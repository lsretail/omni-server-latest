using System;

namespace LSRetail.Omni.Domain.DataModel.Loyalty.Util
{
	public static class MemberContactAttributes
	{
		// Defines if Member Contact attributes are enabled (item1) and required(item2);

		public static class Registration
        {
			public static readonly bool Username = true;      //if not enabled, email is username

            public static readonly Tuple<bool, bool> Email = new Tuple<bool, bool>(true, true);
            public static readonly Tuple<bool, bool> FirstName = new Tuple<bool, bool>(true, true);
            public static readonly Tuple<bool, bool> LastName = new Tuple<bool, bool>(true, true);
			public static readonly Tuple<bool, bool> Address1 = new Tuple<bool, bool>(false, false);
			public static readonly Tuple<bool, bool> Address2 = new Tuple<bool, bool>(false, false);
			public static readonly Tuple<bool, bool> City = new Tuple<bool, bool>(false, false);
			public static readonly Tuple<bool, bool> State = new Tuple<bool, bool>(false, false);
			public static readonly Tuple<bool, bool> PostCode = new Tuple<bool, bool>(false, false);
			public static readonly Tuple<bool, bool> Country = new Tuple<bool, bool>(false, false);
			public static readonly Tuple<bool, bool> Phone = new Tuple<bool, bool>(false, false);
            public static readonly Tuple<bool, bool> DateOfBirth = new Tuple<bool, bool>(false, false);
            public static readonly Tuple<bool, bool> Gender = new Tuple<bool, bool>(false, false);

			public static bool Profiles = true;
		}

		public static class Manage
        {
			public static readonly bool Username = true;      //if not enabled, email is username

			public static readonly Tuple<bool, bool> Email = new Tuple<bool, bool>(true, true);
            public static readonly Tuple<bool, bool> FirstName = new Tuple<bool, bool>(true, true);
            public static readonly Tuple<bool, bool> LastName = new Tuple<bool, bool>(true, true);
            public static readonly Tuple<bool, bool> Address1 = new Tuple<bool, bool>(true, false);
            public static readonly Tuple<bool, bool> Address2 = new Tuple<bool, bool>(true, false);
            public static readonly Tuple<bool, bool> City = new Tuple<bool, bool>(true, false);
            public static readonly Tuple<bool, bool> State = new Tuple<bool, bool>(true, false);
            public static readonly Tuple<bool, bool> PostCode = new Tuple<bool, bool>(true, false);
            public static readonly Tuple<bool, bool> Country = new Tuple<bool, bool>(true, false);
            public static readonly Tuple<bool, bool> Phone = new Tuple<bool, bool>(true, false);
            public static readonly Tuple<bool, bool> DateOfBirth = new Tuple<bool, bool>(false, false);
            public static readonly Tuple<bool, bool> Gender = new Tuple<bool, bool>(false, false);

			public static bool Profiles = true;
		}
	}
}

