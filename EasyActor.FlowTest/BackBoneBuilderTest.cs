using EasyActor.Flow.BackBone;
using EasyActor.Flow.Processor;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using System.Threading;
using System.Threading.Tasks;

namespace EasyActor.FlowTest
{
    [TestFixture]
    public class BackBoneBuilderTest
    {
        private BackBoneBuilder<bool,int> _Builder;
        private IDictionary<Type, object> _Processors;
        private IProcessor<bool, string, int> _Processor;
        private IProgress<int> _Progress;
        private ITransformProcessor<bool, string, int> _Transformer;
        private IProcessorFinalizer<bool, int, int> _IProcessorFinalizer;
        private IBackbone<bool, int> _BackBone;

        [SetUp]
        public void SetUp()
        {
            _Processors = new Dictionary<Type, object>();
            _Processor = Substitute.For<IProcessor<bool, string, int>>();
            _Transformer = Substitute.For<ITransformProcessor<bool, string, int>>();
            _IProcessorFinalizer = Substitute.For<IProcessorFinalizer<bool, int, int>> ();
            _BackBone = Substitute.For<IBackbone<bool, int>>();
            _Progress = Substitute.For<IProgress<int>>();
            _Builder = new BackBoneBuilder<bool, int>(dic =>
            {
                _Processors = dic;
                return null;
            });
        }

        [Test]
        public void Register_IProcessor_AddToDictioanry()
        {
            _Builder.Register(_Processor);
            _Builder.GetBackBone();

            _Processors.Cast<KeyValuePair<Type, object>>().Should().BeEquivalentTo(new KeyValuePair<Type, object>[] 
            {
                new KeyValuePair<Type, object>(typeof(string), _Processor)
            });
        }

        [Test]
        public void Register_ITransformer_AddToDictioanryTransformerProcessorAdapter()
        {
            var boolTRansformer = SetUpForTransormer();

            boolTRansformer.Should().NotBeNull();
        }

        [Test]
        public async Task Register_ITransformer_AddToDictioanryCorrectTransformerProcessorAdapter()
        {
            var boolTRansformer = SetUpForTransormer();
            await boolTRansformer.Process(true, _BackBone, _Progress, CancellationToken.None);

            await _Transformer.Received(1).Transform(true, _Progress, CancellationToken.None);
        }

        private TransformerProcessorAdapter<bool, bool, string, int> SetUpForTransormer()
        {
            _Builder.Register(_Transformer);
            _Builder.GetBackBone();
            return _Processors[typeof(bool)] as TransformerProcessorAdapter<bool, bool, string, int>;
        }

        [Test]
        public void Register_IProcessorFinalizer_AddToDictioanryFinalizerProcessorAdapter()
        {
            var boolTRansformer = SetUpForFinalizer();

            boolTRansformer.Should().NotBeNull();
        }

        [Test]
        public async Task Register_IProcessorFinalizer_AddToDictioanryCorrectFinalizerProcessorAdapter()
        {
            var boolTRansformer = SetUpForFinalizer();
            await boolTRansformer.Process(23, _BackBone, _Progress, CancellationToken.None);

            await _IProcessorFinalizer.Received(1).Process(23, _Progress, CancellationToken.None);
        }

        private ProcessorFinalizerAdapter<bool, int, int> SetUpForFinalizer()
        {
            _Builder.Register(_IProcessorFinalizer);
            _Builder.GetBackBone();
            return _Processors[typeof(int)] as ProcessorFinalizerAdapter<bool, int, int>;
        }
    }
}
