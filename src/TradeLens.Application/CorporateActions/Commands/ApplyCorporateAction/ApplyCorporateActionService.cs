using TradeLens.Application.Interfaces;
using TradeLens.Domain.Services;

namespace TradeLens.Application.CorporateActions.Commands.ApplyCorporateAction;

public sealed class ApplyCorporateActionService
{
    private readonly ICorporateActionRepository _corporateActionRepository;
    private readonly ICorporateActionApplicationRepository _applicationRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IPositionRepository _positionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CorporateActionCalculator _corporateActionCalculator;
    private readonly PositionCalculator _positionCalculator;

    public ApplyCorporateActionService(
        ICorporateActionRepository corporateActionRepository,
        ICorporateActionApplicationRepository applicationRepository,
        ITransactionRepository transactionRepository,
        IPositionRepository positionRepository,
        IUnitOfWork unitOfWork,
        CorporateActionCalculator corporateActionCalculator,
        PositionCalculator positionCalculator)
    {
        _corporateActionRepository = corporateActionRepository;
        _applicationRepository = applicationRepository;
        _transactionRepository = transactionRepository;
        _positionRepository = positionRepository;
        _unitOfWork = unitOfWork;
        _corporateActionCalculator = corporateActionCalculator;
        _positionCalculator = positionCalculator;
    }
}