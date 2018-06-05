using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niqiu.Core.Mapping.User
{
    public class GroupMap : PortalEntityTypeConfiguration<Niqiu.Core.Domain.IM.Group>
    {
        public GroupMap()
        {
            ToTable("Groups");
            HasKey(n => n.Id);
            HasMany(c => c.Users).WithMany(n=>n.Groups).Map(m =>
            {
                m.ToTable("Users_Groups_Mapping");
                m.MapLeftKey("GroupId");
                m.MapRightKey("UserId");
            });
        }

    }
}
