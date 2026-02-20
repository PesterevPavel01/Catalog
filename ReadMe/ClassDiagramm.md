```mermaid
classDiagram

class Order {
    # Guid Id
    # TitleValue Title
    # string Code
    + DateTime CreatedAt
    + DateTime UpdatedAt
    - List~OrderItem~ _orderItems
    - List~OrderEvent~ _orderHistory
    + static short CacheDays$
    + OrderStatus Status
    + IReadOnlyCollection~OrderEvent~ OrderHistory
    + IReadOnlyCollection~OrderItem~ OrderItems
    + ApplicationUser ApplicationUser
    + Guid ApplicationUserId
    + Create(string title, string code, ApplicationUser user) Operation~Order, string~
    + AddOrderEvent(OrderEvent orderEvent) Order
    + AddOrderItem(OrderItem orderItem, IOrderValidator validator) Operation~bool, string~
    + UpdateCode(string newCode) Order
    + RemoveOrderItem(OrderItem orderItem) void
    + IsCompleted() bool
    + IsApprovalCompleted() bool
    + IsCustom() bool
    + IncludeRequiredField()$ Func
    + ConvertToDto() OrderDto
    + Validate(IOrderValidator validator) Operation~bool, string~
    + GenerateUserCommonCacheKey(Func generateCacheKey, string userName)$ string
    + GenerateConstructorCommonCacheKey(Func generateCacheKey)$ string
    + GenerateOrderCacheKey(Func generateCacheKey, string orderCode)$ string
    - CheckCustomization() bool
    - DetermineStatusFromEvent(OrderEventType eventType) OrderStatus?
    - SetStatus(OrderStatus status) Order
    - SetUser(ApplicationUser user) Order
}

class OrderItem {
    # Guid Id
    # TitleValue Title
    # string Code
    + DateTime CreatedAt
    + DateTime UpdatedAt
    - List~Message~ _messages
    + short Quantity
    + IReadOnlyCollection~Message~ Messages
    + Module Module
    + Guid ModuleId
    + Order Order
    + Guid OrderId
    + ApprovalWorkflow ApprovalWorkflow
    + Create(short quantity, Module module)$ Operation~OrderItem, string~
    + SetQuantity(short quantity) void
    + AddMessage(Message message) void
    + IncludeRequiredField()$ Func
    + ConvertToDto() OrderItemDto
    - SetModule(Module module) OrderItem
}

class Module {
    # Guid Id
    # TitleValue Title
    # string Code
    + DateTime CreatedAt
    + DateTime UpdatedAt
    - List~Component~ _components
    - List~OrderItem~ _orderItems
    - List~ModuleTextParameter~ _moduleTextParameters
    - List~ModuleNumericParameter~ _moduleNumericParameters
    + bool IsCustom
    + IReadOnlyCollection~Component~ Components
    + IReadOnlyCollection~OrderItem~ OrderItems
    + IReadOnlyCollection~ModuleTextParameter~ ModuleTextParameters
    + IReadOnlyCollection~ModuleNumericParameter~ ModuleNumericParameters
    + Guid ModuleTypeId
    + ModuleType ModuleType
    + Create(string title, string code, ModuleType moduleType, IModuleParametersValidator validator, List~ModuleTextParameter~ textParams, List~ModuleNumericParameter~ numParams)$ Operation~Module, string~
    + Update(IModuleParametersValidator validator, List~ModuleTextParameter~ textParams, List~ModuleNumericParameter~ numParams, List~Component~ components) Operation~Module, string~
    + AddComponent(Component component, IModuleParametersValidator validator) Operation~ModuleDto, string~
    + RemoveComponent(Component component) Operation~ModuleDto, string~
    + AddTextParameter(ModuleTextParameter textParam) Operation~Module, string~
    + AddNumericParameter(ModuleNumericParameter numParam) Operation~Module, string~
    + RemoveTextParameter(ModuleTextParameter textParam) Operation~Module, string~
    + RemoveNumericParameter(ModuleNumericParameter numParam) Operation~Module, string~
    + ConvertToDto() ModuleDto
    + IncludeRequiredField()$ Func
    - SetModuleType(ModuleType moduleType) Operation~Module, string~
    - CheckCustomization() bool
}

class ModuleType {
    # Guid Id
    # TitleValue Title
    # string Code
    + DateTime CreatedAt
    + DateTime UpdatedAt
    - List~Module~ _modules
    + IReadOnlyCollection~Module~ Modules
    + Create(string title, string code, Guid id)$ Operation~ModuleType, string~
}

class ApplicationUser {
    - List~Message~ _messages
    - List~Order~ _orders
    - List~Role~ _roles
    - List~ApprovalWorkflowItem~ _approvalWorkflowItems
    + string UserName
    + PasswordValue Password
    + string Email
    + UserToken UserToken
    + string ExternalId
    + IReadOnlyCollection~Message~ Messages
    + IReadOnlyCollection~Order~ Orders
    + IReadOnlyCollection~Role~ Roles
    + IReadOnlyCollection~ApprovalWorkflowItem~ ApprovalWorkflowItems
    + Create(Guid id, string userName, string password, string email)$ Operation~ApplicationUser, string~
    + AddRole(Role role) Operation~ApplicationUser, string~
    + CheckPassword(string password) bool
    + SetExternalId(string externalId) Operation~ApplicationUser, string~
    + GenerateCommonCacheKey(Func generateCacheKey)$ string
    - HashPassword(string password)$ string
}

class Message {
    - Message(Guid id, string text)
    + ApplicationUser ApplicationUser
    + Guid ApplicationUserId
    + OrderItem OrderItem
    + Guid OrderItemId
    + string Text
    + Create(string text, OrderItem orderItem, ApplicationUser applicationUser)$ Operation~Message, string~
    + IncludeRequiredField()$ Func
    + ConvertToDto() MessageDto
}

class OrderEvent {
    # Guid Id
    # TitleValue Title
    # string Code
    + DateTime CreatedAt
    + DateTime UpdatedAt
    + static short CacheEntriesCount$ = 20
    + Order Order
    + Guid OrderId
    + int Type
    + Create(string title, OrderEventType type, string code)$ Operation~OrderEvent, string~
    + ConvertToDto() OrderEventDto
    + IncludeRequiredField()$ Func
    + GenerateUserCommonCacheKey(Func generateCacheKey, string userName)$ string
    + GenerateConstructorCommonCacheKey(Func generateCacheKey)$ string
}

class OrderEventType {
    <<enumeration>>
    ApprovalCompleted
    Cancelled
    Changed
    CreateApprovalWorkflow
    Created
    CustomModuleModified
    Disabled
    Deleted
    MessageAdded
    Reject
    Exported
    ExternallyRejected
    Produced
    Completed
}

class OrderStatus {
    <<enumeration>>
    Draft
    PendingApproval
    ApprovalCompleted
    InProduction
    RejectedFromProduction
    Produced
    Shipped
    Delivered
    Completed
    Cancelled
}

class ApprovalWorkflow {
    - List~ApprovalWorkflowItem~ _approvalWorkflowItems
    + static string CompletedStageCode$
    + bool IsCompleted
    + IReadOnlyCollection~ApprovalWorkflowItem~ ApprovalWorkflowItems
    + ApprovalWorkflowItem ActiveStage
    + OrderItem OrderItem
    + Guid OrderItemId
    + Create(string title, string code, OrderItem orderItem, ApprovalStage startStage, ApplicationUser user)$ Operation~ApprovalWorkflow, string~
    + Approve(ApplicationUser user, ApprovalStage stage, short number) Operation~ApprovalWorkflowItem, string~
    + Complete(ApplicationUser user, ApprovalStage completedStage) Operation~ApprovalWorkflowItem, string~
    + ConvertToDto() ApprovalWorkflowDto
    + IncludeRequiredField()$ Func
    - CheckIsCompleted() bool
}

class ApprovalStage {
    - List~ApprovalWorkflowItem~ _approvalWorkflowItems
    + IReadOnlyCollection~ApprovalWorkflowItem~ ApprovalWorkflowItems
    + Create(string title, string code)$ Operation~ApprovalStage, string~
    + ConvertToDto() SimpleEntityDto
}

class ApprovalWorkflowItem {
    + short Number
    + ApprovalStage ApprovalStage
    + Guid ApprovalStageId
    + ApprovalWorkflow ApprovalWorkflow
    + Guid ApprovalWorkflowId
    + ApplicationUser Initiator
    + Guid InitiatorId
    + Create(ApplicationUser user, ApprovalStage stage, short number)$ Operation~ApprovalWorkflowItem, string~
    + ConvertToDto() ApprovalWorkflowItemDto
    + IncludeRequiredField()$ Func
}

class Component {
    + ComponentType ComponentType
    + List~ComponentNumericParameter~ ComponentNumericParameters
    + List~ComponentTextParameter~ ComponentTextParameters
    + bool IsCustom
    + ConvertToDto() ComponentDto
}

class ComponentType {
    + string Name
    + string Code
}

class ComponentNumericParameter {
    + double Value
    + ParameterType ParameterType
}

class ComponentTextParameter {
    + string Value
    + ParameterType ParameterType
}

class ModuleNumericParameter {
    + double Value
    + ParameterType ParameterType
}

class ModuleTextParameter {
    + string Value
    + ParameterValueType ParameterType
}

class ParameterValueType {
    <<enumeration>>
    Numeric
    Text
}

Order "many" --> "1" ApplicationUser
Order "1" *-- "many" OrderItem
OrderEvent "many" --> "1" Order
OrderItem "1" *-- "many" Module
Module "many" --> "many" Component
Component "1" *-- "many" ComponentType
Component "1" *-- "many" ComponentNumericParameter
Component "1" *-- "many" ComponentTextParameter
Module "1" *-- "many" ModuleNumericParameter
Module "1" *-- "many" ModuleTextParameter
ModuleType "1" *-- "many" Module
OrderItem "1" *-- "many" Message
OrderEventType --> OrderEvent
OrderStatus --> Order
Message "many" --> "1" ApplicationUser
ApprovalWorkflowItem "many" --> "1" ApprovalWorkflow
ApprovalWorkflow "1" --> "1" OrderItem
ApprovalStage "1" *-- "many" ApprovalWorkflowItem
ApprovalWorkflowItem "many" --> "1" ApplicationUser

```