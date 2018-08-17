using System;
using System.Collections.Generic;
using LSRetail.Omni.Domain.DataModel.Base.Utils;

namespace LSOmni.DataAccess.Interface.Repository.Loyalty
{
    public interface IEmailMessageRepository
    {
        int MaxRetryCounter();
        string Save(EmailMessage email);
        EmailMessage EmailMessageGetById(string id);
        void UpdateStatus(string guid, EmailStatus status,string errorMessage);
        List<EmailMessage> EmailMessageSearch(DateTime? dateFrom, DateTime? dateTo, EmailStatus? status);
    }
}
