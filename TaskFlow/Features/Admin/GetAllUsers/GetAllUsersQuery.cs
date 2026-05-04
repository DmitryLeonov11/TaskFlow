using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Features.Admin.GetAllUsers;

public class GetAllUsersQuery : IRequest<IReadOnlyList<AdminUserDto>>
{
}
