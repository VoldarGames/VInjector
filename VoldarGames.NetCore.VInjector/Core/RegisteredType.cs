﻿using System;

namespace VoldarGames.NetCore.VInjector.Core
{
    internal class RegisteredType
    {
        public Type InterfaceType { get; set; }
        public string RegistrationName { get; set; }
        public int Priority { get; set; }
        public LifeTime LifeTime { get; set; }
    }
}