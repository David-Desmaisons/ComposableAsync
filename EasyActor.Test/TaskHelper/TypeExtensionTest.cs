using System;
using System.Threading.Tasks;

using FluentAssertions;
using NUnit.Framework;

using EasyActor.TaskHelper;

namespace EasyActor.Test
{
    [TestFixture]
    public class TypeExtensionTest
    {
         [Test]
        public void GetDelegateTypeNoTask()
        {
            Type st = typeof(string);

            var target = st.GetTaskType();

            target.MethodType.Should().Be(TaskType.None);
            target.Type.Should().BeNull();
        }

          [Test]
        public void GetDelegateTypeTask()
        {
            Type st = typeof(Task);

            var target = st.GetTaskType();

            target.MethodType.Should().Be(TaskType.Task);
            target.Type.Should().BeNull();
        }

           [Test]
        public void GetDelegateTypeGenericTask()
        {
            Type st = typeof(Task<string>);

            var target = st.GetTaskType();

            target.MethodType.Should().Be(TaskType.GenericTask);
            target.Type.Should().Be(typeof(string));
        }
    }
}
