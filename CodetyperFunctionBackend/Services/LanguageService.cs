using CodetyperFunctionBackend.Model;
using CodetyperFunctionBackend.Repositories;

namespace CodetyperFunctionBackend.Services
{
    internal class LanguageService
    {
        private readonly LanguageRepository _languageRepository;

        public LanguageService(LanguageRepository languageRepository)
        {
            _languageRepository = languageRepository;
        }

        public async Task<(bool Success, string Message)> AddLanguageAsync(string languageName)
        {
            if (await _languageRepository.LanguageExistsAsync(languageName))
            {
                return (false, $"Language '{languageName}' already exists.");
            }

            await _languageRepository.AddLanguageAsync(languageName);
            return (true, $"Language '{languageName}' added successfully.");
        }

        public async Task<IEnumerable<Language>> GetAllLanguagesAsync()
        {
            var languages = await _languageRepository.GetAllLanguagesAsync();

            return languages.OrderBy(language => language.Name).ToList();
        }

        public async Task<Language?> GetLanguageByNameAsync(string languageName)
        {
            return await _languageRepository.getLanguageByNameAsync(languageName);
        }
    }
}
