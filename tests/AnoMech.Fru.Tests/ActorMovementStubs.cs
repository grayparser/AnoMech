using System.Numerics;
using AnoMech.Core.Game;
using AnoMech.Core.Game.Party;
// Minimal engine stand-ins for executing the production AI and EventScheduler.
// These record movement requests; the geometric tests separately simulate travel.
// They do not pretend to test the native client or the real SimWorld lifecycle.
namespace AnoMech.Core.SimObjects
{
    public partial class SimCharacter : IPositioned
    {
        public bool IsActive { get; set; } = true;
        public bool Dead { get; set; }
        public PartyRole Role { get; set; }
        public Vector3 Position { get; set; }
        public float Rotation { get; set; }
        public List<(Vector3 Target, float Speed)> Moves { get; } = [];
        public void MoveTo(Vector3 target, float speed) => Moves.Add((target, speed));
    }
    public sealed partial class SimPlayer : SimCharacter, ISimPartyMember { }
    public sealed class SimPartyNpc : SimCharacter, ISimPartyMember
    {
        public byte Level { get; set; }
    }
    public sealed partial class SimEnemy : SimCharacter
    {
        public SimCharacter? Target { get; private set; }
        public bool Following { get; private set; }
        public void SetTarget(SimCharacter? target, bool follow = true)
        {
            Target = target;
            Following = follow;
        }
    }
    public sealed class SimParty
    {
        public PartyFinder Find => new(this);
        public void WipeAllPlayers(string cause)
        {
            foreach (var m in Members) { m.Damage.Add((0, cause, true)); m.Dead = true; }
        }
        public List<SimCharacter> Members { get; } = [];
        public SimPlayer? Player => Members.OfType<SimPlayer>().SingleOrDefault();
        // The harness contains manual players and bots, without network puppets or debug autoplay.
        public bool IsBotDriven(SimCharacter member) => !ReferenceEquals(member, Player);
        public PartyRole PlayerRole => Player?.Role ?? PartyRole.MainTank;
        public SimCharacter? Get(PartyRole role) => Members.FirstOrDefault(member => member.Role == role);
        public IEnumerable<SimCharacter> ActiveMembers() => Members.Where(member => !member.Dead);
    }
    public sealed class PartyFinder(SimParty party)
    {
        public List<SimCharacter> InsideCircle(Vector3 center, float radius)
            => party.ActiveMembers().Where(m => Vector3.Distance(m.Position, center) <= radius).ToList();
        public List<SimCharacter> InsideRect(Placement p, float halfWidth, float length)
        {
            var forward = new Vector3(MathF.Sin(p.Rotation), 0, MathF.Cos(p.Rotation));
            var right = new Vector3(forward.Z, 0, -forward.X);
            return party.ActiveMembers().Where(m =>
            {
                var d = m.Position - p.Position; var z = Vector3.Dot(d, forward);
                return z >= 0 && z <= length && MathF.Abs(Vector3.Dot(d, right)) <= halfWidth;
            }).ToList();
        }
    }
    public sealed partial class SimWorld
    {
        public EventScheduler Events { get; } = new();
        public SimParty Party { get; } = new();
    }
}
