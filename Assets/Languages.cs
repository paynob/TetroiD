using System.Collections.Generic;

public class Languages {

	public enum Language {English, Spanish};
	
	static LanguageClass current;
	
	class LanguageClass{
		Dictionary<string,string> pair;
		string languageName;
		string code;

		public static LanguageClass English(){
			LanguageClass lc = new LanguageClass ();
			lc.languageName = "English";
			lc.code = "EN";
			lc.pair = new Dictionary<string, string> ();
			lc.pair.Add ("_LOCAL_", "Local");
			lc.pair.Add ("_ROOMNAME_", "Room Name");
			lc.pair.Add ("_JOIN_", "Join");
			lc.pair.Add ("_CREATE_", "Create");
			lc.pair.Add ("_READY_", "I'm ready");
			lc.pair.Add ("_BACK_", "Back");
			lc.pair.Add ("_AWIN_", "Arkanoid win");
			lc.pair.Add ("_TWIN_", "Tetris win");
			lc.pair.Add ("_WIN_", "You win");
			lc.pair.Add ("_LOSE_", "You lose");
			lc.pair.Add ("_ROUNDSTARTS_", "Round starts in");
			lc.pair.Add ("_LANG_", lc.languageName);
			lc.pair.Add ("_LANGCODE_", lc.code);
			return lc;
		}

		public static LanguageClass Spanish(){
			LanguageClass lc = new LanguageClass ();
			lc.languageName = "Español";
			lc.code = "ES";
			lc.pair = new Dictionary<string, string> ();
			lc.pair.Add ("_LOCAL_", "Local");
			lc.pair.Add ("_ROOMNAME_", "Sala");
			lc.pair.Add ("_JOIN_", "Unirse");
			lc.pair.Add ("_CREATE_", "Crear");
			lc.pair.Add ("_READY_", "Listo");
			lc.pair.Add ("_BACK_", "Atras");
			lc.pair.Add ("_AWIN_", "Arkanoid ganó");
			lc.pair.Add ("_TWIN_", "Tetris ganó");
			lc.pair.Add ("_WIN_", "Has ganado");
			lc.pair.Add ("_LOSE_", "Has perdido");
			lc.pair.Add ("_ROUNDSTARTS_", "La ronda comienza en");
			lc.pair.Add ("_LANG_", lc.languageName);
			lc.pair.Add ("_LANGCODE_", lc.code);
			return lc;
		}

		public string GetValue (string key){
			string value = "ERROR";
			if (pair.ContainsKey (key))
				pair.TryGetValue (key, out value);
			return value;
		}

	}

	public static void SetLanguage(Language lang){
		switch (lang) {
		case Language.Spanish:
			current = LanguageClass.Spanish();
			break;
		case Language.English:
			current = LanguageClass.English();
			break;
		}
	}

	public static string Translate( string key ){
		if (current == null)
			SetLanguage (Language.English);
		return current.GetValue(key);
	}
}
