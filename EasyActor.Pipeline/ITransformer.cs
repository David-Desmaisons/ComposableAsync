using System.Threading.Tasks;

namespace EasyActor.Pipeline
{
    public interface ITransformer<Tin,Tout>
    {
        Task<Tout> Transform(Tin entry);
    }
}
