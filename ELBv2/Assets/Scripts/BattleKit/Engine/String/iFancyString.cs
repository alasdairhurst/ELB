
namespace BattleKit.Engine {
	public interface iFancyString {
		string ToString(StringOpts opts, int tabIndex);
	}

	/* 
	 *	OneLine
	 *		{ Data }
     *
	 *	Short
	 *		{ OneLine ... (n) }
	 *
	 *	TwoLine
	 *		Class
	 *		{ Short }
	 *
	 *	Pretty
	 *		{ 
	 *			Pretty 
	 *		}
	 *
	 *	Full
	 *		Class
	 *		{ Short }
	 *		{ 
	 *			Pretty 
	 *		}
	 *	
	 *	Identifier
	 *		EditorID (Id)
	 *
	 */
	public enum StringOpts {
		OneLine = 0,
		TwoLine = 1,
		Pretty = 2,
		Full = 3,
		Short = 4,
		Identifier = 5
	}
}
