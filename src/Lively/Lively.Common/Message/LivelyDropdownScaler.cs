﻿using System;

namespace Lively.Common.Message
{
    [Serializable]
    public class LivelyDropdownScaler : IpcMessage
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public LivelyDropdownScaler() : base(MessageType.lp_dropdown_scaler)
        {
        }
    }
}
