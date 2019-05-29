using Common.Logging;
using Splitio.Domain;
using Splitio.Services.InputValidation.Interfaces;
using System;
using System.Collections.Generic;

namespace Splitio.Services.InputValidation.Classes
{
    public class EventPropertiesValidator : IEventPropertiesValidator
    {
        private const int MAX_PROPERTIES_LENGTH_BYTES = 32 * 1024;
        protected readonly ILog _log;

        public EventPropertiesValidator(ILog log)
        {
            _log = log;
        }

        public EventValidatorResult IsValid(Dictionary<string, object> properties)
        {
            if (properties == null) return new EventValidatorResult { Success = true };

            var propertiesResult = new Dictionary<string, object>();
            var size = 1024L;

            if (properties.Count > 300)
            {
                _log.Warn("Event has more than 300 properties. Some of them will be trimmed when processed");
            }

            foreach (var entry in properties)
            {
                size += entry.Key.Length;
                var value = entry.Value;

                if (value == null) continue;

                if (!IsNumeric(value) && !(value is bool) && !(value is string))
                {
                    _log.Warn($"Property {value.GetType()} is of invalid type. Setting value to null");
                    value = null;
                }

                if (value is string)
                {
                    size += ((string)value).Length;
                }

                if (size > MAX_PROPERTIES_LENGTH_BYTES)
                {
                    _log.Error($"The maximum size allowed for the properties is 32768 bytes. Current one is {size} bytes. Event not queued");
                    return new EventValidatorResult { Success = false };
                }

                propertiesResult.Add(entry.Key, value);
            }

            return new EventValidatorResult
            {
                Success = true,
                Value = propertiesResult,
                EventSize = size
            };
        }

        private bool IsNumeric(object value)
        {
            var valueTypeCode = Type.GetTypeCode(value.GetType());

            switch (valueTypeCode)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}
