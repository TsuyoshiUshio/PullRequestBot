using System.Threading.Tasks;

namespace PullRequestLibrary.Command
{
    public interface ICommand
    {
        Task Execute(ICommandContext context);
    }
}