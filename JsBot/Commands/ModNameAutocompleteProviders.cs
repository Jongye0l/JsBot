using JALib.Server.Models;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace JsBot.Commands;

public class AllModNameAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext> {
	public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context) {
		string typed = (option.Value ?? "").ToLower();
		IEnumerable<ApplicationCommandOptionChoiceProperties> choices = ModData.GetModDataList()
			.Select(mod => mod.Name)
			.Where(name => name.ToLower().StartsWith(typed))
			.Take(25)
			.Select(name => new ApplicationCommandOptionChoiceProperties(name, name));
		return ValueTask.FromResult<IEnumerable<ApplicationCommandOptionChoiceProperties>?>(choices);
	}
}

public class PublicJModNameAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext> {
	public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context) {
		string typed = (option.Value ?? "").ToLower();
		IEnumerable<ApplicationCommandOptionChoiceProperties> choices = JMod.GetModList()
			.Where(mod => !mod.PrivateMod && mod.Name.ToLower().StartsWith(typed))
			.Take(25)
			.Select(mod => new ApplicationCommandOptionChoiceProperties(mod.Name, mod.Name));
		return ValueTask.FromResult<IEnumerable<ApplicationCommandOptionChoiceProperties>?>(choices);
	}
}

public class AllJModNameAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext> {
	public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context) {
		string typed = (option.Value ?? "").ToLower();
		IEnumerable<ApplicationCommandOptionChoiceProperties> choices = JMod.GetModList()
			.Where(mod => mod.Name.ToLower().StartsWith(typed))
			.Take(25)
			.Select(mod => new ApplicationCommandOptionChoiceProperties(mod.Name, mod.Name));
		return ValueTask.FromResult<IEnumerable<ApplicationCommandOptionChoiceProperties>?>(choices);
	}
}