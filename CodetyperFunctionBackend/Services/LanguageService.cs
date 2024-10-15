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

        public async Task<(bool success, string message)> AddLanguageAsync(Language language)
        {
            if (language == null)
                return (false, "Language cannot be null.");
            else if (string.IsNullOrEmpty(language.Name))
                return (false, "Language name cannot be null or empty.");
            else if (await _languageRepository.LanguageExistsAsync(language.Name))
                return (false, $"Language '{language.Name}' already exists.");

            await _languageRepository.AddLanguageAsync(language.Name);
            return (true, $"Language '{language.Name}' added successfully.");
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
