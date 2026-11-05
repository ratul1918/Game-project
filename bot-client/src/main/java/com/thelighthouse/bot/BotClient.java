package com.thelighthouse.bot;

import java.net.URI;
import java.net.http.*;
import java.net.http.WebSocket.*;
import java.time.*;
import java.util.*;
import java.util.concurrent.*;
import java.util.concurrent.atomic.*;

public final class BotClient {
    public static void main(String[] args) throws Exception {
        Arguments options = Arguments.parse(args);
        ExecutorService pool = Executors.newVirtualThreadPerTaskExecutor();
        List<Future<BotResult>> results = new ArrayList<>();
        for (int i = 0; i < options.count; i++) {
            int botId = i;
            long delay = options.rampSeconds * 1000L * i / Math.max(1, options.count);
            results.add(pool.submit(() -> { Thread.sleep(delay); return new Bot(options.url, botId).run(options.duration); }));
        }
        pool.shutdown();
        List<BotResult> completed = new ArrayList<>();
        for (Future<BotResult> result : results) completed.add(result.get());
        BotResult.writeCsv(options.csv, completed);
    }

    record Arguments(String url, int count, int duration, int rampSeconds, String csv) {
        static Arguments parse(String[] args) {
            Map<String,String> values = new HashMap<>();
            for (int i = 0; i + 1 < args.length; i += 2) values.put(args[i], args[i + 1]);
            return new Arguments(values.getOrDefault("--url", "http://localhost:8080"),
                    Integer.parseInt(values.getOrDefault("--bots", "10")),
                    Integer.parseInt(values.getOrDefault("--duration", "30")),
                    Integer.parseInt(values.getOrDefault("--ramp-up", "5")),
                    values.getOrDefault("--csv", "bot-results.csv"));
        }
    }

    static final class Bot {
        private final String baseUrl; private final int id; private final AtomicLong lastSeq = new AtomicLong();
        private final List<Long> latencies = new CopyOnWriteArrayList<>(); private final AtomicInteger gaps = new AtomicInteger(); private final AtomicInteger outOfOrder = new AtomicInteger();
        Bot(String baseUrl, int id) { this.baseUrl = baseUrl.replaceAll("/$", ""); this.id = id; }
        BotResult run(int seconds) throws Exception {
            String username = "bot-" + id + "-" + System.nanoTime(), password = "bot-password-123";
            HttpClient http = HttpClient.newHttpClient();
            String body = "{\"username\":\"" + username + "\",\"password\":\"" + password + "\"}";
            http.send(HttpRequest.newBuilder(URI.create(baseUrl + "/auth/register")).header("Content-Type","application/json").POST(HttpRequest.BodyPublishers.ofString(body)).build(), HttpResponse.BodyHandlers.ofString());
            String response = http.send(HttpRequest.newBuilder(URI.create(baseUrl + "/auth/login")).header("Content-Type","application/json").POST(HttpRequest.BodyPublishers.ofString(body)).build(), HttpResponse.BodyHandlers.ofString()).body();
            String token = response.replaceAll(".*\"token\":\"([^\"]+)\".*", "$1");
            CountDownLatch closed = new CountDownLatch(1);
            WebSocket socket = http.newWebSocketBuilder().buildAsync(URI.create(baseUrl.replace("http","ws") + "/ws?token=" + token),
                    new Listener() {
                        public void onOpen(WebSocket ws) { ws.request(1); }
                        public CompletionStage<?> onText(WebSocket ws, CharSequence data, boolean last) {
                            String text = data.toString(); long seq = parseLong(text, "\"seq\":", -1);
                            long previous = lastSeq.getAndSet(seq);
                            if (previous > 0 && seq > previous + 1) gaps.incrementAndGet();
                            if (previous > seq) outOfOrder.incrementAndGet();
                            if (text.contains("\"type\":\"PONG\"")) { long sent = parseLong(text, "\"sent\":", 0); if (sent > 0) latencies.add(System.nanoTime() - sent); }
                            ws.request(1); return null;
                        }
                        public void onError(WebSocket ws, Throwable error) { closed.countDown(); }
                        public CompletionStage<?> onClose(WebSocket ws, int status, String reason) { closed.countDown(); return null; }
                    }).get(10, TimeUnit.SECONDS);
            long end = System.nanoTime() + seconds * 1_000_000_000L; Random random = new Random(id);
            String[] acts = {"A1-B01","A2-B04","A3-B02","A4-B05","A5-B03"};
            while (System.nanoTime() < end) {
                send(socket, "NOISE_EMITTED", "{\"loudness\":4.5}");
                send(socket, "LAMP_STATE", "{\"state\":\"Failing\",\"charge\":0.2}");
                send(socket, "PLAYER_HID", "{\"hiding\":true}");
                send(socket, "BEAT_REACHED", "{\"id\":\"" + acts[random.nextInt(acts.length)] + "\"}");
                long sent = System.nanoTime();
                socket.sendText("{\"type\":\"PING\",\"seq\":0,\"payload\":{\"sent\":" + sent + "}}", true);
                Thread.sleep(100L + random.nextInt(300));
            }
            socket.sendClose(WebSocket.NORMAL_CLOSURE, "done").join(); closed.await(2, TimeUnit.SECONDS);
            return new BotResult(id, latencies, gaps.get(), outOfOrder.get());
        }
        private static void send(WebSocket socket, String type, String payload) { socket.sendText("{\"type\":\"" + type + "\",\"seq\":0,\"payload\":" + payload + "}", true); }
        private static long parseLong(String value, String key, long fallback) { int start=value.indexOf(key); if(start<0)return fallback; start+=key.length(); int end=start; while(end<value.length()&&Character.isDigit(value.charAt(end)))end++; try{return Long.parseLong(value.substring(start,end));}catch(Exception e){return fallback;} }
    }

    record BotResult(int id, List<Long> latencies, int gaps, int outOfOrder) {
        static void writeCsv(String path, List<BotResult> results) throws Exception {
            List<Long> values = results.stream().flatMap(r -> r.latencies.stream()).sorted().toList();
            long p50 = percentile(values,.50), p95=percentile(values,.95), p99=percentile(values,.99);
            long gaps=results.stream().mapToLong(BotResult::gaps).sum(), order=results.stream().mapToLong(BotResult::outOfOrder).sum();
            java.nio.file.Files.writeString(java.nio.file.Path.of(path), "bots,p50_latency_ms,p95_latency_ms,p99_latency_ms,seq_gaps,out_of_order\n" +
                    results.size()+","+p50+","+p95+","+p99+","+gaps+","+order+"\n");
            System.out.printf("bots=%d p50=%dms p95=%dms p99=%dms seqGaps=%d outOfOrder=%d%n",results.size(),p50,p95,p99,gaps,order);
        }
        private static long percentile(List<Long> values,double p){if(values.isEmpty())return 0;return values.get(Math.min(values.size()-1,(int)Math.ceil(values.size()*p)-1))/1_000_000;}
    }
}
