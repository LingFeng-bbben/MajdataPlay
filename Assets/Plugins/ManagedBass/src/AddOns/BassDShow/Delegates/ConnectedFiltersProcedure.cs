using System;

namespace ManagedBass.DShow
{
	/// <summary>
	/// User defined callback function to receive connected filters (see <see cref="BassDShow.ChannelGetConnectedFilters" />).
	/// </summary>
	/// <param name="Filter">A pointer to the connected filter instance (IBaseFilter). Note that this instance should be used inside the callback as it is released automaticaly once the callback returned.</param>
	/// <param name="FilterName">The name of the connected filter.</param>
	/// <param name="HasPropertyPage"><see langword="true" /> if the filter has a property page (dialog), <see langword="false" /> if not.</param>
	/// <param name="User">The user instance data given when <see cref="BassDShow.ChannelGetConnectedFilters" /> was called.</param>
	/// <returns><see langword="true" /> to continue, else <see langword="false" />.</returns>
	public delegate bool ConnectedFiltersProcedure(IntPtr Filter, string FilterName, bool HasPropertyPage, IntPtr User);
}