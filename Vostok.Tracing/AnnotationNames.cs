namespace Vostok.Tracing
{
    // CR(iloktionov): Move to abstractions alongside with design document.
    public static class AnnotationNames
    {
        public const string Operation = "operation";
        public const string Host = "host";
        public const string SpanKind = "kind";
        public const string Component = "component";
        public const string Service = "service";

        public static class Http
        {
            public static class Request
            {
                public const string Method = "http.request.method";
                public const string Url = "http.request.url";
                public const string ContentLength = "http.request.content-length";
            }

            public static class Response
            {
                public const string StatusCode = "http.response.status-code";
                public const string ContentLength = "http.response.content-length";
            }

            public static class Client
            {
                public const string Name = "http.client.name";
                public const string Address = "http.client.address";
            }
        }

        public static class Queue
        {
            public const string Type = "queue.type";
            public const string Topic = "queue.topic";
            public const string ActionResult = "queue.action-result";
            public const string TaskId = "queue.task-id";
            public const string TaskTraceId = "queue.task-trace-id";
            public const string ExecutionResult = "queue.execution-result";

            public static class Source
            {
                public const string TraceId = "queue.source.trace-id";
                public const string SpanId = "queue.source.span-id";
            }
        }

        public static class Cluster
        {
            public const string RequestStrategy = "cluster.request-strategy";
            public const string Status = "cluster.status";
        }

        public static class Database
        {
            public const string Type = "db.type";
            public const string ExecutionResult = "db.execution-result";
            public const string Instance = "db.instance";
        }
    }
}