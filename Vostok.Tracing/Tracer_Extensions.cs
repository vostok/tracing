using System;
using System.Net.Http;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    public static class Tracer_Extensions
    {
        public static ISpanBuilder BeginHttpRequestClientSpan(this ITracer tracer, Uri uri, HttpMethod httpMethod, int contentLength)
        {
            var spanBuilder = tracer.BeginSpan();

            spanBuilder.SetAnnotation(AnnotationNames.Http.Request.Url, uri);
            spanBuilder.SetAnnotation(AnnotationNames.Http.Request.Method, httpMethod.Method);
            spanBuilder.SetAnnotation(AnnotationNames.Http.Request.ContentLength, contentLength);

            //TODO: normalize url
            spanBuilder.SetAnnotation(AnnotationNames.Operation, $"({httpMethod.Method}) {uri}");
            spanBuilder.SetAnnotation(AnnotationNames.SpanKind, "http-request-client");

            return spanBuilder;
        }
    }

    // public interface IHttpRequestSpanBuilder : ISpanBuilder
    // {
        // void SetRequestDetails(Uri url, ...);

        // void SetResponseDetails(int responseCode, ...);
    // }
}