﻿using ScoreManagement.Model.Table;
using ScoreManagement.Model;

namespace ScoreManagement.Interfaces
{
    public interface IMasterDataQuery
    {
        Task<List<SystemParam>> GetSystemParams(string reference);
        Task<Dictionary<string, string>> GetLanguage(string language);
        Task<List<EmailPlaceholder>> GetEmailPlaceholder();
        Task<List<EmailTemplate>> GetEmailTemplate(string username);
        Task<Dictionary<string,int?>> GetDefaultEmailTemplate(string username);
        Task<List<SubjectResponse>> GetSubject();
        Task<List<NotificationTemplateResponse>> GetNotifyTemplate();
    }
}
