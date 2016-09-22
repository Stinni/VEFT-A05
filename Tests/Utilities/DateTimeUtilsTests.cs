using CoursesAPI.Services.Utilities;
using Xunit;

namespace CoursesAPI.Tests.Utilities
{
	public class DateTimeUtilsTests
	{
		#region IsLeapYear

		/// <summary>
		/// Years not divisible by 4 are definitely not leap years:
		/// </summary>
		[Fact]
		public void IsLeapYear_TestForNonLeapYear()
		{
			// Act:
			var result = DateTimeUtils.IsLeapYear(2015);

			// Assert:
			Assert.False(result);
		}

		/// <summary>
		/// Years divisible by 4 are usually leap years
		/// (but see below).
		/// </summary>
		[Fact]
		public void IsLeapYear_TestForStandardLeapYearDivisibleBy4()
		{
			// Act:
			var result = DateTimeUtils.IsLeapYear(2012);

			// Assert:
			Assert.True(result);
		}

		/// <summary>
		/// Years divisible by 400 are always leap years:
		/// </summary>
		[Fact]
		public void IsLeapYear_TestForLeapYearDivisibleBy400()
		{
			// Act:
			var result = DateTimeUtils.IsLeapYear(1600);

			// Assert:
			Assert.True(result);
		}

		/// <summary>
		/// Years divisible by 100 are never leap years,
		/// unless they are divisible by 400:
		/// </summary>
		[Fact]
		public void IsLeapYear_TestForNonLeapYearDivisibleBy100()
		{
			// Act:
			var result = DateTimeUtils.IsLeapYear(1900);

			// Assert:
			Assert.False(result);
		}

		/// <summary>
		/// Finally, throw in yet another test, just for good measure...
		/// </summary>
		[Fact]
		public void IsLeapYear_TestForNonLeapYearEdgeCase1()
		{
			// Act:
			var result = DateTimeUtils.IsLeapYear(1999);

			// Assert:
			Assert.False(result);
		}

		#endregion
	}
}
