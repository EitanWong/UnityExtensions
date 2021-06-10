#region copyright
// ---------------------------------------------------------------
//  Copyright (C) Dmitriy Yukhanov - focus [https://codestage.net]
// ---------------------------------------------------------------
#endregion

namespace CodeStage.Maintainer.UI
{
	using UnityEngine;

	internal abstract class TwoColumnsTab : BaseTab
	{
		protected Vector2 leftColumnScrollPosition;
		protected Vector2 rightColumnScrollPosition;

		protected TwoColumnsTab(MaintainerWindow window) : base(window) {}

        public virtual void Refresh(bool newData)
        {
	        leftColumnScrollPosition = Vector2.zero;
	        rightColumnScrollPosition = Vector2.zero;
        }

        public virtual void Draw()
        {
	        using (new GUILayout.HorizontalScope())
	        {
		        DrawLeftColumn();
		        DrawRightColumn();
	        }
        }

        private void DrawLeftColumn()
        {
	        using (new GUILayout.VerticalScope(UIHelpers.panelWithBackground, GUILayout.Width(240)))
	        {
		        DrawLeftColumnPanel();
	        }
        }

        private void DrawLeftColumnPanel()
        {
	        DrawLeftColumnHeader();
	        DrawLeftColumnBody();
        }

        protected virtual void DrawLeftColumnHeader()
        {

        }

        protected virtual void DrawLeftColumnBody() { }

        protected virtual void DrawRightColumn()
        {
	        using (new GUILayout.VerticalScope(UIHelpers.panelWithBackground))
	        {
		        DrawRightColumnBody();
	        }
        }

        protected virtual void DrawRightColumnBody()
        {
	        if (!DrawRightColumnTop())
		        return;

	        GUILayout.Space(5);

	        if (!DrawRightColumnCenter())
		        return;

	        GUILayout.Space(10);

	        DrawRightColumnBottom();
        }

        protected virtual bool DrawRightColumnTop()
        {
	        return true;
        }

        protected virtual bool DrawRightColumnCenter()
        {
	        return true;
        }
        protected virtual void DrawRightColumnBottom() { }
	}
}