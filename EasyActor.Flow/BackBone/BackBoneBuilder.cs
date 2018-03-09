using EasyActor.Flow.Processor;
using System;
using System.Collections.Generic;

namespace EasyActor.Flow.BackBone
{
    public class BackBoneBuilder<TRes, TTProgress> : IBackboneBuilder<TRes, TTProgress>
    {
        private readonly IDictionary<Type, object> _Processors = new Dictionary<Type, object>();
        private readonly Func<IDictionary<Type, object>, IBackbone<TRes, TTProgress>> _Builder;

        public BackBoneBuilder(): this(dic => new BackBone<TRes, TTProgress>(dic))
        {
        }

        internal BackBoneBuilder(Func<IDictionary<Type, object>, IBackbone<TRes, TTProgress>> builder)
        {
            _Builder = builder;
        }

        public void Register<TMessage>(IProcessor<TRes, TMessage, TTProgress> processor)
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
        }

        public void Register<TMessage>(IProcessorFinalizer<TRes, TMessage, TTProgress> processor)
        {
            Register(new ProcessorFinalizerAdapter<TRes, TMessage, TTProgress>(processor));
        }

        public void Register<TMessage1, TMessage2>(ITransformProcessor<TMessage1, TMessage2, TTProgress> processor)
        {
            Register(new TransformerProcessorAdapter<TRes, TMessage1, TMessage2, TTProgress>(processor));
        }

        public IBackbone<TRes, TTProgress> GetBackBone()
        {
            return _Builder(_Processors);
        }
    }
}
