
namespace ELB.Utils {
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
	 *	Id
	 *		_Id
	 *	
	 *	EditorID
	 *		_EditorID
	 *	
	 *	IdCombo
	 *		EditorID (Id)
	 *
	 */
	public enum StringOpts {
		OneLine = 0,
		TwoLine = 1,
		Pretty = 2,
		Full = 3,
		Short = 4,
		Id = 5,
		EditorID = 6,
		IdCombo = 7
	}
}
