namespace BattleKit.Editor {
	class SearchableItemList : ItemList {
		private string _searchTerm = "";

		private bool filter(string term) {
			if(_searchTerm == "" || _searchTerm == null) {
				return true;
			}
			return term.Contains(_searchTerm);
		}

		public new void Start( ) {
			_searchTerm = CustomFields.SearchField(_searchTerm);
			base.Start();
		}

		public SelectionType FoldoutGroup(string title, bool doFilter = true) {
			if(!doFilter || filter(title)) {
				return base.FoldoutGroup(title);
			}
			return SelectionType.None;
		}

		public SelectionType ListItem(string title, bool doFilter = true, bool doSomething = true) {
			if(!doFilter || filter(title)) {
				return base.ListItem(title, doSomething);
			}
			return SelectionType.None;
		}
	}

}
