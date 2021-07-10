using Catalyst;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DelusionalApi.Model
{
    public class Random<T>
    {
        protected readonly List<T> UsedItems = new List<T>(); 

        public T Next(params T[] options)
        {
            var returnValue = options
                .OrderBy(o => Guid.NewGuid())
                .First(o => !UsedItems.Contains(o));


            UsedItems.Add(returnValue);

            return returnValue;
        }
    }
}