﻿using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators
{
    internal sealed class LinkedListConverterCreator : IConverterCreator
    {
        public IConverter GetConverter(IGeneratorContext context, Type type)
        {
            return CommonHelper.GetConverter(context, type, typeof(LinkedList<>), typeof(LinkedListConverter<>), null);
        }
    }
}
