using System;
using System.Collections.Generic;
using Concurrent.Flow.Processor;

namespace Concurrent.Flow.BackBone
{
    public class BackBoneBuilder<TRes, TProgress> : IBackboneBuilder<TRes, TProgress>
    {
        private readonly IDictionary<Type, object> _Processors = new Dictionary<Type, object>();
        private readonly Func<IDictionary<Type, object>, IBackbone<TRes, TProgress>> _Builder;

        public BackBoneBuilder(): this(dic => new BackBone<TRes, TProgress>(dic))
        {
        }

        internal BackBoneBuilder(Func<IDictionary<Type, object>, IBackbone<TRes, TProgress>> builder)
        {
            _Builder = builder;
        }

        public IBackboneBuilder<TRes, TProgress> Register<TMessage>(IProcessor<TRes, TMessage, TProgress> processor)
        {
            var messageType = typeof(TMessage);
            try
            {
                _Processors.Add(messageType, processor);
            }
            catch(ArgumentException)
            {
                throw new ArgumentException($"A processor of same message type ({messageType}) has already been registered!", nameof(processor));
            }
            return this;
        }

        public IBackboneBuilder<TRes, TProgress> Register<TMessage>(IProcessorFinalizer<TRes, TMessage, TProgress> processor)
        {
            Register(new ProcessorFinalizerAdapter<TRes, TMessage, TProgress>(processor));
            return this;
        }

        public IBackboneBuilder<TRes, TProgress> Register<TMessage1, TMessage2>(ITransformProcessor<TMessage1, TMessage2, TProgress> processor)
        {
            Register(new TransformerProcessorAdapter<TRes, TMessage1, TMessage2, TProgress>(processor));
            return this;
        }

        public IBackbone<TRes, TProgress> GetBackBone()
        {
            return _Builder(_Processors);
        }
    }
}
