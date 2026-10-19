# Thread safety

`SessionRegistry` uses `ConcurrentHashMap.newKeySet()` so WebSocket open and
close callbacks can add/remove sessions while the scheduled world clock
broadcasts. `Broadcaster` broadcasts over a snapshot of that set, removes
closed or failed sessions, and synchronizes on each session around
`sendMessage`, preventing concurrent writes to one WebSocket connection.

`WorldClock.tick()` is synchronized because it owns mutable threat and raid
state. Its message sequence is an `AtomicLong`, so all messages get a unique
increasing `seq` even if future publishers run concurrently.
