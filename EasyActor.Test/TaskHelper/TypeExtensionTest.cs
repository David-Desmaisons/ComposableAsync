﻿using System;
using System.Threading.Tasks;
using EasyActor.TaskHelper;
using FluentAssertions;
using Xunit;

namespace EasyActor.Test.TaskHelper
{

    public class TypeExtensionTest
    {
        [Fact]
        public void GetDelegateTypeNoTask()
        {
            Type st = typeof(string);

            var target = st.GetTaskType();

            target.MethodType.Should().Be(TaskType.None);
            target.Type.Should().BeNull();
        }

        [Fact]
        public void GetDelegateTypeTask()
        {
            Type st = typeof(Task);

            var target = st.GetTaskType();

            target.MethodType.Should().Be(TaskType.Task);
            target.Type.Should().BeNull();
        }

        [Fact]
        public void GetDelegateTypeGenericTask()
        {
            Type st = typeof(Task<string>);

            var target = st.GetTaskType();

            target.MethodType.Should().Be(TaskType.GenericTask);
            target.Type.Should().Be(typeof(string));
        }


        [Fact]
        public void GetDelegateVoidType()
        {
            Type st = typeof(void);

            var target = st.GetTaskType();

            target.MethodType.Should().Be(TaskType.Void);
            target.Type.Should().BeNull();
        }
    }
}
