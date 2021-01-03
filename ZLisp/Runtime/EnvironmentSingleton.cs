using System;

namespace ZLisp.Runtime
{
    public sealed class EnvironmentSingleton
    {
        private EnvironmentSingleton()
        {
            foreach (var entry in Runtime.ns)
            {
                Env.Set(new Language.Syntax.Types.Symbol(entry.Key), entry.Value);
            }
        }

        private static readonly Lazy<EnvironmentSingleton> lazy = new Lazy<EnvironmentSingleton>(() => new EnvironmentSingleton());
        public static EnvironmentSingleton Instance { get => lazy.Value; }
        public Environment Env = new Environment(null);
    }
}
