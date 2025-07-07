using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.DataService;
using System.Threading;

namespace Hermes.Parking.Server.RequestProcessor
{
    public class SecurityDataProvider : BaseDataProvider
    {
        private ILogger Logger;
        private IDataBaseService db;
        private ISecurityManager sm;

        public SecurityDataProvider(IContext Context)
        {
            this.Logger = Context.GetService<ILogger>("Logger");
            this.db = Context.GetService<IDataBaseService>(Hermes.Parking.Server.DataService.Constants.DATABASE_SERVICE_NAME);
            this.sm = Context.GetService<ISecurityManager>(Hermes.Parking.Server.RequestProcessor.Constants.SECURITY_MANAGER_NAME);
        }

        public override string GetName()
        {
            return Constants.SECURITY_DATA_PROVIDER_NAME;
        }

        public override void Create(string ObjectType, object InitialObject, ref object Output)
        {
            switch (ObjectType)
            {
                case Constants.USER_ROLE_OBJECT_TYPE_NAME:
                    Output = CreateUserRole(InitialObject);
                    break;

                case Constants.USER_OBJECT_TYPE_NAME:
                    Output = CreateUser(InitialObject);
                    break;
            }
        }

        public override void GetList(string ObjectType, IDictionary<string, object> Filter, ref IEnumerable<object> Output)
        {
            switch (ObjectType)
            {
                case Constants.REQUEST_OBJECT_TYPE_NAME:
                    Output = GetAllRequests();
                    break;

                case Constants.USER_ROLE_OBJECT_TYPE_NAME:
                    Output = GetAllUserRoles();
                    break;

                case Constants.USER_OBJECT_TYPE_NAME:
                    Output = GetAllUsers();
                    break;
            }
        }

        public override void Save(string ObjectType, object Object)
        {
            switch (ObjectType)
            {
                case Constants.USER_ROLE_OBJECT_TYPE_NAME:
                    SaveUserRole((UserRole)Object);
                    break;

                case Constants.USER_OBJECT_TYPE_NAME:
                    SaveUser((User)Object);
                    break;
            }
        }

        public override void Delete(string ObjectType, object Object)
        {
            switch (ObjectType)
            {
                case Constants.USER_ROLE_OBJECT_TYPE_NAME:
                    DeleteUserRole((UserRole)Object);
                    break;

                case Constants.USER_OBJECT_TYPE_NAME:
                    DeleteUser((User)Object);
                    break;
            }
        }


        private IEnumerable<ServerRequest> GetAllRequests()
        {
            SqlCommand cmd = new SqlCommand(@"SELECT * FROM Request");
            DataSet sqlResult = db.GetDataSet(cmd);
            IList<ServerRequest> result = new List<ServerRequest>();
            foreach (DataRow row in sqlResult.Tables[0].Rows)
            {
                int id = (int)row["Id"];
                int groupId = (int)row["GroupId"];
                string name = (string)row["Name"];
                string description = (string)row["Description"];
                ServerRequest operation = new ServerRequest() { Id = id, Group = (RequestGroup)groupId, Name = name, Description = description };
                result.Add(operation);
            }
            return result;
        }


        #region UserRoles

        private UserRole CreateUserRole(object InitialObject)
        {
            if (!(InitialObject is UserRole))
                throw new InvalidCastException("Not 'IUserRole' object given");

            UserRole role = (UserRole)InitialObject;
            SqlCommand cmd = new SqlCommand(@"
                DECLARE @RoleId int

                INSERT INTO UserRole(Name) VALUES (@Name)
                SELECT @RoleId = SCOPE_IDENTITY()

                MERGE UserRoleRequest AS T
                USING @Requests AS S
                ON T.RoleId = @RoleId AND T.RequestId = S.Value
                WHEN NOT MATCHED THEN INSERT (RoleId, RequestId) VALUES (@RoleId, S.Value)
                WHEN NOT MATCHED BY SOURCE AND T.RoleId = @RoleId THEN DELETE;

                SELECT * FROM UserRole WHERE Id = @RoleId;
                SELECT * FROM UserRoleRequest WHERE RoleId = @RoleId
                ");

            cmd.Parameters.Add("@Name", SqlDbType.VarChar).Value = role.Name;
            SqlParameter requests = new SqlParameter("@Requests", SqlDbType.Structured);
            requests.TypeName = "IntTable";
            requests.Value = CreateRequestIdsTable(role.Requests);
            cmd.Parameters.Add(requests);

            DataSet sqlResult = db.GetDataSet(cmd);
            UserRole result = ParseUserRole(sqlResult.Tables[0].Rows[0]);

            List<Tuple<int, int>> permissions = ParseUserRoleRequests(sqlResult.Tables[1]);
            List<int> operationIds = permissions.Where(x => x.Item1 == result.Id).Select(x => x.Item2).ToList();
            result.Requests = sm.GetAllRequests().Where(x => operationIds.Contains(x.Id)).Select(x => x.Name).ToList();

            return result;
        }

        private IEnumerable<UserRole> GetAllUserRoles()
        {
            SqlCommand cmd = new SqlCommand(@"SELECT * FROM UserRole; SELECT * FROM UserRoleRequest");
            DataSet sqlResult = db.GetDataSet(cmd);

            List<Tuple<int, int>> permissions = ParseUserRoleRequests(sqlResult.Tables[1]);

            IList<UserRole> result = new List<UserRole>();
            foreach (DataRow row in sqlResult.Tables[0].Rows)
            {
                UserRole role = ParseUserRole(row);
                List<int> operationIds = permissions.Where(x => x.Item1 == role.Id).Select(x => x.Item2).ToList();
                role.Requests = sm.GetAllRequests().Where(x => operationIds.Contains(x.Id)).Select(x => x.Name).ToList();
                result.Add(role);
            }
            return result;
        }

        private void SaveUserRole(UserRole Role)
        {
            SqlCommand cmd = new SqlCommand(@"
                UPDATE UserRole SET Name = @Name WHERE Id = @RoleId;

                MERGE UserRoleRequest AS T
                USING @Requests AS S
                ON T.RoleId = @RoleId AND T.RequestId = S.Value
                WHEN NOT MATCHED THEN INSERT (RoleId, RequestId) VALUES (@RoleId, S.Value)
                WHEN NOT MATCHED BY SOURCE AND T.RoleId = @RoleId THEN DELETE;");

            cmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = Role.Id;
            cmd.Parameters.Add("@Name", SqlDbType.VarChar).Value = Role.Name;

            SqlParameter operations = new SqlParameter("@Requests", SqlDbType.Structured);
            operations.TypeName = "IntTable";
            operations.Value = CreateRequestIdsTable(Role.Requests);
            cmd.Parameters.Add(operations);

            db.Execute(cmd);
        }

        private void DeleteUserRole(UserRole Role)
        {
            SqlCommand cmd = new SqlCommand(@"DELETE FROM UserRole WHERE Id = @RoleId");
            cmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = Role.Id;
            db.Execute(cmd);
        }

        private UserRole ParseUserRole(DataRow Row)
        {
            int id = (int)Row["Id"];
            string name = (string)Row["Name"];

            UserRole result = new UserRole()
            {
                Id = id,
                Name = name,
                Requests = new List<string>()
            };

            return result;
        }

        private List<Tuple<int, int>> ParseUserRoleRequests(DataTable UserRoleOperations)
        {
            List<Tuple<int, int>> result = new List<Tuple<int, int>>();

            foreach (DataRow row in UserRoleOperations.Rows)
                result.Add(new Tuple<int, int>((int)row["RoleId"], (int)row["RequestId"]));

            return result;
        }

        #endregion


        #region Users

        private User CreateUser(object InitialObject)
        {
            if (!(InitialObject is User))
                throw new InvalidCastException("Not 'IUser' object given");

            User user = (User)InitialObject;
            SqlCommand cmd = new SqlCommand(@"
                DECLARE @UserId int

                INSERT INTO [User] (TypeId, [Login], [Password], IsActive) VALUES (@TypeId, @Login, @Password, @IsActive)
                SELECT @UserId = SCOPE_IDENTITY()

                MERGE UserRoleLink AS T
                USING @Roles AS S
                ON T.UserId = @UserId AND T.RoleId = S.Value
                WHEN NOT MATCHED THEN INSERT (UserId, RoleId) VALUES (@UserId, S.Value)
                WHEN NOT MATCHED BY SOURCE AND T.UserId = @UserId THEN DELETE;

                SELECT * FROM [User] WHERE Id = @UserId;
                SELECT * FROM UserRoleLink WHERE UserId = @UserId
                ");

            cmd.Parameters.Add("@TypeId", SqlDbType.Int).Value = (int)user.Type;
            cmd.Parameters.Add("@Login", SqlDbType.VarChar).Value = user.Login;
            cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = user.Password;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = user.IsActive;
            SqlParameter roles = new SqlParameter("@Roles", SqlDbType.Structured);
            roles.TypeName = "IntTable";
            roles.Value = CreateIdsDataTable(user.Roles);
            cmd.Parameters.Add(roles);

            DataSet sqlResult = db.GetDataSet(cmd);

            User result = ParseUser(sqlResult.Tables[0].Rows[0]);

            List<Tuple<int, int>> userRoles = ParseUserRoles(sqlResult.Tables[1]);
            result.Roles = userRoles.Where(x => x.Item1 == result.Id).Select(x => x.Item2).ToList();

            return result;
        }

        private IEnumerable<User> GetAllUsers()
        {
            SqlCommand cmd = new SqlCommand(@"SELECT * FROM [User]; SELECT * FROM UserRoleLink");
            DataSet sqlResult = db.GetDataSet(cmd);

            List<Tuple<int, int>> roles = ParseUserRoles(sqlResult.Tables[1]);

            IList<User> result = new List<User>();
            foreach (DataRow row in sqlResult.Tables[0].Rows)
            {
                User user = ParseUser(row);
                user.Roles = roles.Where(x => x.Item1 == user.Id).Select(x => x.Item2).ToList();
                result.Add(user);
            }
            return result;
        }

        private void SaveUser(User User)
        {
            SqlCommand cmd = new SqlCommand(@"
                UPDATE [User] SET TypeId = @TypeId, [Login] = @Login, [Password] = ISNULL(@Password, [Password]), IsActive = @IsActive WHERE Id = @UserId;

                MERGE UserRoleLink AS T
                USING @Roles AS S
                ON T.UserId = @UserId AND T.RoleId = S.Value
                WHEN NOT MATCHED THEN INSERT (UserId, RoleId) VALUES (@UserId, S.Value)
                WHEN NOT MATCHED BY SOURCE AND T.UserId = @UserId THEN DELETE;");

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = User.Id;
            cmd.Parameters.Add("@TypeId", SqlDbType.Int).Value = User.Type;
            cmd.Parameters.Add("@Login", SqlDbType.VarChar).Value = User.Login;
            cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = (string.IsNullOrEmpty(User.Password)) ? (object)DBNull.Value : User.Password;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = User.IsActive;

            SqlParameter roles = new SqlParameter("@Roles", SqlDbType.Structured);
            roles.TypeName = "IntTable";
            roles.Value = CreateIdsDataTable(User.Roles);
            cmd.Parameters.Add(roles);

            db.Execute(cmd);
        }

        private void DeleteUser(User User)
        {
            SqlCommand cmd = new SqlCommand(@"DELETE FROM [User] WHERE Id = @UserId");
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = User.Id;
            db.Execute(cmd);
        }

        private User ParseUser(DataRow Row)
        {
            int id = (int)Row["Id"];
            int typeId = (int)Row["TypeId"];
            string login = (string)Row["Login"];
            string password = (string)Row["Password"];
            bool isActive = (bool)Row["IsActive"];

            User result = new User()
            {
                Id = id,
                Login = login,
                Password = password,
                Type = (UserType)typeId,
                IsActive = isActive
            };

            return result;
        }

        private List<Tuple<int, int>> ParseUserRoles(DataTable UserRoles)
        {
            List<Tuple<int, int>> result = new List<Tuple<int, int>>();

            foreach (DataRow row in UserRoles.Rows)
                result.Add(new Tuple<int, int>((int)row["UserId"], (int)row["RoleId"]));

            return result;
        }

        #endregion


        private DataTable CreateIdsDataTable(IEnumerable<int> Ids)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Value", typeof(int));
            if (Ids != null)
                foreach (int id in Ids)
                    table.Rows.Add(id);
            return table;
        }

        private DataTable CreateRequestIdsTable(IEnumerable<string> Names)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Value", typeof(int));
            if (Names != null)
                foreach (string name in Names)
                    table.Rows.Add(sm.GetRequestByName(name).Id);
            return table;
        }
    }
}
