﻿using System.Collections.Generic;

namespace Schedule.Model
{
    public class ClassInput
    {
        public string Name { get; set; }
        public string Faculty { get; set; }
        public List<string> Subjects { get; set; } = new();
    }
}
