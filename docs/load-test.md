# Backend load test

Run `scripts/load-test.sh` on the target machine (an 8 GB MacBook Air M3). It
starts PostgreSQL and the server, then runs the bot client at 10, 50, 100, 200
and 500 concurrent WebSockets. Results are written to `docs/load-test/`.

Measured on the 8 GB MacBook Air M3 with a 3 second run and 1 second ramp:

| bots | p50 latency (ms) | p95 (ms) | p99 (ms) | seq gaps |
|---:|---:|---:|---:|---:|
| 10 | 393 | 962 | 982 | 0 |
| 50 | 468 | 937 | 992 | 0 |
| 100 | 431 | 939 | 991 | 0 |
| 200 | 517 | 983 | 1137 | 0 |
| 500 | 516 | 1020 | 1906 | 0 |

All five levels completed. At 500 bots the server snapshot measured tick p95
`0.000063456 s`, maximum observed outbound queue depth `10`, `49` dropped
messages, and `17,514` dropped telemetry events. CPU and heap were not captured
by this run, so no values are claimed for them.

The CSV contains bot latency and sequence-order observations. The server JSON
snapshot also contains tick p95, queue depth, dropped messages, telemetry
drops, and reconnects. CPU and heap should be added from the host's Activity
Monitor or an equivalent profiler when recording a run.

The observed bottleneck was telemetry database throughput: the bounded queue
dropped events at 500 bots while ticks remained fast and bot sequence gaps stayed
at zero. Telemetry was made asynchronous with a bounded queue and batch writer
so database stalls do not block the world-clock thread; a full queue drops the
oldest event.
