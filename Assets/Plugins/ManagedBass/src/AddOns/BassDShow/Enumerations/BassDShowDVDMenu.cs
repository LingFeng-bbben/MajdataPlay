namespace ManagedBass.DShow
{
	/// <summary>
	/// DVD menu options used by <see cref="BassDShow.DVDChannelMenu" />.
	/// </summary>
	public enum BassDShowDVDMenu
	{
		/// <summary>
		/// Select MENU from Point.
		/// <para>value1 parameter should be the x coordonate.</para>
		/// <para>value2 parameter should be the y coordonate.</para>
		/// </summary>
		SelectAtPos = 21,
		
		/// <summary>
		/// Activate Button from Point.
		/// <para>value1 parameter should be the x coordonate.</para>
		/// <para>value2 parameter should be the y coordonate.</para>
		/// </summary>
		ActionAtPos = 22,
		
		/// <summary>
		/// Handle a enter key down event.
		/// <para>value1 parameter is ignored (should be 0).</para>
		/// <para>value2 parameter is ignored (should be 0).</para>
		/// </summary>
		ActivateButton = 23,
		
		/// <summary>
		/// Handle arrow key events.
		/// <para>value1: 0=VK_LEFT, -1=VK_RIGHT, -2=VK_UP, -3=VK_DOWN.</para>
		/// <para>value2 parameter is ignored (should be 0).</para>
		/// </summary>
		SelectButton = 24
	}
}