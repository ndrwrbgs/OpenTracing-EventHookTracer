﻿namespace OpenTracing.Contrib.EventHookTracer
{
    using System;
    using JetBrains.Annotations;

    internal sealed class EventHookScope : IScope
    {
        private readonly bool finishSpanOnDispose;
        private readonly string spanOperationName;
        private readonly EventHandler<EventHookTracer.LogEventArgs> spanLog;
        private readonly EventHandler<EventHookTracer.SetTagEventArgs> spanSetTag;
        private readonly Action onDispose;
        [NotNull] private readonly EventHookTracer tracer;
        private readonly IScope impl;

        public EventHookScope(
            [NotNull] IScope impl,
            [NotNull] EventHookTracer tracer,
            bool finishSpanOnDispose,
            string spanOperationName,
            EventHandler<EventHookTracer.LogEventArgs> spanLog,
            EventHandler<EventHookTracer.SetTagEventArgs> spanSetTag,
            Action onDispose)
        {
            this.impl = impl;
            this.tracer = tracer;
            this.finishSpanOnDispose = finishSpanOnDispose;
            this.spanOperationName = spanOperationName;
            this.spanLog = spanLog;
            this.spanSetTag = spanSetTag;
            this.onDispose = onDispose;
        }

        public void Dispose()
        {
            if (this.finishSpanOnDispose)
            {
                this.tracer.OnSpanFinishing(this.Span);
            }

            this.impl.Dispose();
            this.onDispose?.Invoke();

            if (this.finishSpanOnDispose)
            {
                this.tracer.OnSpanFinished(this.Span);
            }
        }

        ISpan IScope.Span => this.Span;

        internal EventHookSpan Span
        {
            get
            {
                ISpan span = this.impl.Span;
                var wrap = new EventHookSpan(span, this.tracer, this.spanOperationName, this.spanLog, this.spanSetTag, null);
                return wrap;
            }
        }
    }
}