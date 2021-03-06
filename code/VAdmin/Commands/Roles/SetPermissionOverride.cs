using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAdmin.Commands
{
	public class SetPermissionOverride : VAdminCommand
	{
		public override string Name => "permission_set_override";

		public override string Description => "Revokes a permission to a role.";

		public override string Category => "Role Management";

		public override EchoType Echo => EchoType.ToStaff;

		public override CommandArg[] Args => new CommandArg[] {
			new CommandArg("role name", CommandArgType.Role),
			new CommandArg("permission", CommandArgType.Permission),
			new CommandArg("override", CommandArgType.Bool)
		};

		public override bool Execute( Client caller, string[] args, out string error )
		{
			var roleName = args[0];
			var permissionId = args[1];
			var set = bool.Parse( args[2] );

			if ( !VAdminSystem.FindRoleByName( roleName, out var role ) )
			{
				error = $"Role '{roleName}' not found.";
				return false;
			}

			var perm = role.GetPermission( permissionId );
			if ( perm.Priority == set )
			{
				error = $"Override already set to '{set}'";
				return false;
			}

			perm.Priority = set;

			if ( !VAdminSystem.TrySetRolePermission( role.Id, perm, out error ) )
			{
				return false;
			}

			VAdminSystem.SaveRoles();
			VAdminSystem.NetworkRole( To.Everyone, role );

			return true;
		}

		public override void OnExecuteFailure( Client caller, string[] args, string error )
		{
			if ( !VAdminSystem.FindRoleByName( args[0], out var role ) )
			{
				return;
			}

			if ( caller != null )
			{
				// Re-network the role to correct client mistake in UI.
				VAdminSystem.NetworkRole( To.Single( caller ), role );
			}
		}

		public override string FormatMessage( Client caller, string[] args )
		{
			var roleName = args[0];
			var permissionId = args[1];
			var set = args[2];

			return $"Set permission '{permissionId}' override: {set}";
		}
	}
}
