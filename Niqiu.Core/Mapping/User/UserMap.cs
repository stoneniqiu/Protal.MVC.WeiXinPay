namespace Niqiu.Core.Mapping.User
{
    public class UserMap : PortalEntityTypeConfiguration<Domain.User.User>
    {
        public UserMap()
        {
            ToTable("Users");
            HasKey(n => n.Id);
            Property(n => n.Username).HasMaxLength(100);
            Property(n => n.Email).HasMaxLength(500);
           // HasMany(n=>n.Groups).WithRequired(n=>n.Owner).WillCascadeOnDelete(false);
            HasMany(c => c.UserRoles).WithMany().Map(m => m.ToTable("User_UserRole_Mapping"));
        }
    }
}
 
