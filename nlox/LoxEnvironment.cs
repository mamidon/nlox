using System.Collections.Generic;

namespace nlox
{
	public class LoxEnvironment
	{
		readonly LoxEnvironment _enclosingEnvironment;
		readonly IDictionary<string, object> _state;

		public LoxEnvironment()
		{
			_enclosingEnvironment = null;
			_state = new Dictionary<string, object>();
		}

		public LoxEnvironment(LoxEnvironment environment)
		{
			_enclosingEnvironment = environment;
			_state = new Dictionary<string, object>();
		}

		public void Define(string name, object value)
		{
			_state[name] = value;
		}

		public void Assign(Token name, object value)
		{
			if (_state.ContainsKey(name.Lexeme)) {
				_state[name.Lexeme] = value;
				return;
			}

			if (_enclosingEnvironment != null) {
				_enclosingEnvironment.Assign(name, value);
				return;
			}
			
			throw new LoxRuntimeErrorException(name, $"Undefined variable '{name.Lexeme}'");
		}

		public void Bind(LoxInstance instance)
		{
			_state["this"] = instance;
		}

		public object Get(Token identifier)
		{
			if (_state.ContainsKey(identifier.Lexeme)) {
				return _state[identifier.Lexeme];
			}

			if (_enclosingEnvironment != null) {
				return _enclosingEnvironment.Get(identifier);
			}
			
			throw new LoxRuntimeErrorException(identifier, $"Undefined variable '{identifier.Lexeme}'");
		}

		public object GetAt(int distance, Token name)
		{
			return GetAncestor(distance).Get(name);
		}
		
		
		public void AssignAt(int distance, Token name, object value)
		{
			GetAncestor(distance).Assign(name, value);
		}

		LoxEnvironment GetAncestor(int distance)
		{
			var environment = this;

			for (var i = 0; i < distance; i++) {
				environment = environment._enclosingEnvironment;
			}

			return environment;
		}
	}
}