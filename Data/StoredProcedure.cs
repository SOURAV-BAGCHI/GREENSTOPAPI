using System;

namespace GreenStop.API.Data
{
    public class StoredProcedure
    {
        public const String usp_CatagoryItemDetails_Get="usp_CatagoryItemDetails_Get" ;
        public const String usp_EmployeeByRole_GetList="usp_EmployeeByRole_GetList {0},{1},{2}";
        public const String usp_AspNetUsers_GetUserDetails="usp_AspNetUsers_GetUserDetails {0}";
        public const String usp_AspNetUsers_CheckIfUserExist="usp_AspNetUsers_CheckIfUserExist {0}";
        public const String usp_Token_InsertUpdate="usp_Token_InsertUpdate {0},{1}";
        public const String usp_AspNetUsers_OTPLoginAndRefToken="usp_AspNetUsers_OTPLoginAndRefToken {0}, {1}";
        public const String usp_AspNetRoles_GetRoles="usp_AspNetRoles_GetRoles";
        public const String usp_Order_GetList="usp_Order_GetList {0},{1},{2},{3},{4},{5},{6}";
        public const String usp_OrderDetails_SetAction="usp_OrderDetails_SetAction {0},{1},{2},{3}";
        public const String usp_GetCurrentOrderDetails="usp_GetCurrentOrderDetails {0},{1},{2},{3},{4},{5}";
        public const String usp_GetCurrentOrderDetailsCount="usp_GetCurrentOrderDetailsCount {0},{1},{2},{3}";
        public const String usp_GetOrderDeliveryDetails="usp_GetOrderDeliveryDetails {0},{1}";
        public const String usp_OrderDetailsSetCustomerServiceStatus="usp_OrderDetailsSetCustomerServiceStatus {0},{1}";
        public const String usp_OrderDetails_GetOrderDetailsMinCount="usp_OrderDetails_GetOrderDetailsMinCount {0}";
        public const String usp_OrderDetails_GetOrderDetailsMin="usp_OrderDetails_GetOrderDetailsMin {0},{1},{2}";
        public const String usp_GetOrderDetailsHistoryCount="usp_GetOrderDetailsHistoryCount {0},{1},{2},{3}";
        public const String usp_GetOrderDetailsHistory="usp_GetOrderDetailsHistory {0},{1},{2},{3},{4},{5}";
        public const String usp_AspNetUsers_GetEmployeeCount="usp_AspNetUsers_GetEmployeeCount {0}";
        public const String usp_AspNetUsers_GetEmployee="usp_AspNetUsers_GetEmployee {0},{1},{2}";
        public const String usp_UserFeedbacks_GetCount="usp_UserFeedbacks_GetCount";
        public const String usp_UserFeedbacks_GetUserFeedbacks="usp_UserFeedbacks_GetUserFeedbacks {0},{1}";
    }
}