#!/usr/bin/env python3
"""
Query Azure Application Insights / Log Analytics for the last 24 hours of
production exceptions, grouped by type and message with total counts.

Requirements:
    pip install azure-monitor-query azure-identity

Authentication — DefaultAzureCredential is used, which tries in order:
    1. Environment variables:  AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, AZURE_TENANT_ID
    2. Azure CLI:              az login
    3. Managed Identity (when running on Azure)

Finding your Workspace ID:
    Azure Portal → Application Insights → <your resource> → Properties
    → "Log Analytics Workspace" → click through → Properties → Workspace ID (GUID)

    Or set it as an env var:
        export AZURE_WORKSPACE_ID="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"

Usage:
    python insights_exceptions.py --workspace-id <GUID>
    python insights_exceptions.py --workspace-id <GUID> --hours 48
    python insights_exceptions.py --workspace-id <GUID> --query-file custom.kql
    python insights_exceptions.py --workspace-id <GUID> --csv exceptions.csv
"""

import argparse
import csv
import os
import sys
from datetime import timedelta

from azure.identity import DefaultAzureCredential
from azure.monitor.query import LogsQueryClient, LogsQueryStatus


def build_query(hours: int) -> str:
    return f"""
exceptions
| where timestamp > ago({hours}h)
| summarize
    Count         = count(),
    FirstSeen     = min(timestamp),
    LastSeen      = max(timestamp)
  by
    ExceptionType  = type,
    InnerMessage   = innermostMessage,
    InnerMethod    = innermostMethod,
    OuterMessage   = outerMessage
| order by Count desc
"""


def run(workspace_id: str, query: str, hours: int, csv_path: str | None) -> None:
    credential = DefaultAzureCredential()
    client = LogsQueryClient(credential)

    print(f"Querying workspace {workspace_id!r} — last {hours} hour(s)...\n", flush=True)

    response = client.query_workspace(
        workspace_id=workspace_id,
        query=query,
        timespan=timedelta(hours=hours),
    )

    if response.status == LogsQueryStatus.FAILURE:
        print(f"ERROR: Query failed.\n{response.partial_error}", file=sys.stderr)
        sys.exit(1)

    if not response.tables or not response.tables[0].rows:
        print("No exceptions found in the specified time range.")
        return

    table = response.tables[0]
    col_names = [c.name for c in table.columns]

    required = {"ExceptionType", "InnerMessage", "InnerMethod", "OuterMessage", "Count", "FirstSeen", "LastSeen"}
    missing = required - set(col_names)
    if missing:
        print(f"ERROR: Result is missing expected columns: {missing}", file=sys.stderr)
        print(f"Columns returned: {col_names}", file=sys.stderr)
        sys.exit(1)

    idx = {name: col_names.index(name) for name in required}

    rows = []
    for row in table.rows:
        rows.append({
            "count":        int(row[idx["Count"]] or 0),
            "type":         str(row[idx["ExceptionType"]] or "").strip() or "(unknown)",
            "inner_msg":    str(row[idx["InnerMessage"]]  or "").strip(),
            "inner_method": str(row[idx["InnerMethod"]]   or "").strip(),
            "outer_msg":    str(row[idx["OuterMessage"]]  or "").strip(),
            "first_seen":   str(row[idx["FirstSeen"]]     or ""),
            "last_seen":    str(row[idx["LastSeen"]]      or ""),
        })

    _print_table(rows, hours)

    if csv_path:
        _write_csv(rows, csv_path)
        print(f"\nResults saved to: {csv_path}")


def _print_table(rows: list[dict], hours: int) -> None:
    sep_wide = "=" * 130
    sep_thin = "-" * 130

    print(sep_wide)
    print(f"  PRODUCTION EXCEPTIONS — last {hours} hour(s)   ({len(rows)} distinct exception group(s))")
    print(sep_wide)

    for i, r in enumerate(rows, 1):
        print(f"\n  #{i}  COUNT: {r['count']}")
        print(f"  {'Exception Type':<16} {r['type']}")
        print(f"  {'Inner Message':<16} {r['inner_msg'] or '(none)'}")
        print(f"  {'Inner Method':<16} {r['inner_method'] or '(none)'}")
        if r["outer_msg"]:
            print(f"  {'Outer Message':<16} {r['outer_msg']}")
        print(f"  {'First Seen':<16} {r['first_seen']}")
        print(f"  {'Last Seen':<16} {r['last_seen']}")
        if i < len(rows):
            print(sep_thin)

    print(f"\n{sep_wide}")
    total = sum(r["count"] for r in rows)
    print(f"  Total exception occurrences : {total}")
    print(f"  Distinct exception groups   : {len(rows)}")
    print(sep_wide)


def _write_csv(rows: list[dict], path: str) -> None:
    fields = ["count", "type", "inner_msg", "inner_method", "outer_msg", "first_seen", "last_seen"]
    headers = ["Count", "ExceptionType", "InnerMessage", "InnerMethod", "OuterMessage", "FirstSeen", "LastSeen"]
    with open(path, "w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=fields)
        f.write(",".join(headers) + "\n")
        writer.writerows(rows)


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Report production exceptions from Azure Application Insights (last N hours).",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    parser.add_argument(
        "--workspace-id",
        default=os.getenv("AZURE_WORKSPACE_ID"),
        metavar="GUID",
        help="Log Analytics workspace ID (UUID). Falls back to AZURE_WORKSPACE_ID env var.",
    )
    parser.add_argument(
        "--hours",
        type=int,
        default=24,
        metavar="N",
        help="Hours back to query (default: 24).",
    )
    parser.add_argument(
        "--query-file",
        metavar="FILE",
        help="Path to a .kql file with a custom query. Must return columns: "
             "ExceptionType, InnerMessage, InnerMethod, OuterMessage, Count, FirstSeen, LastSeen.",
    )
    parser.add_argument(
        "--csv",
        metavar="FILE",
        help="Also write results to this CSV file.",
    )

    args = parser.parse_args()

    if not args.workspace_id:
        parser.error(
            "Workspace ID required. Use --workspace-id or set AZURE_WORKSPACE_ID."
        )

    if args.query_file:
        with open(args.query_file, encoding="utf-8") as f:
            query = f.read()
    else:
        query = build_query(args.hours)

    run(
        workspace_id=args.workspace_id,
        query=query,
        hours=args.hours,
        csv_path=args.csv,
    )


if __name__ == "__main__":
    main()
