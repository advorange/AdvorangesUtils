namespace AdvorangesUtils
{
	/// <summary>
	/// What to do when duplicate sizes are found in image metadata.
	/// </summary>
	public enum DuplicateSizeMethod
	{
		/// <summary>
		/// Return the smallest values of each.
		/// </summary>
		Minimum,
		/// <summary>
		/// Return the largest values of each.
		/// </summary>
		Maximum,
		/// <summary>
		/// Return the first values of each.
		/// </summary>
		First,
		/// <summary>
		/// Return the last values of each.
		/// </summary>
		Last,
		/// <summary>
		/// Throw an exception.
		/// </summary>
		Throw,
	}
}