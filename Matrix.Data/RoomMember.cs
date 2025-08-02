
namespace Matrix.Business.Room;

public class RoomMember
{
    public string AvatarUrl { get; set; }
    public string DisplayName { get; set; }
    public string Membership { get; set; }
    public string Reason { get; set; }
    public string ThirdPartyInvite { get; set; }
    public string JoinAuthorizedViaUsersServer { get; set; }
    public bool IsDirect {get; set;}
    
}