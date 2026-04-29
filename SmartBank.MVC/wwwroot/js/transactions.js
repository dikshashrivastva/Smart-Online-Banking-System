const form = document.querySelector("[data-balance-refresh]");
const accountSelect = document.querySelector("[data-account-select]");
const balanceOutput = document.querySelector("[data-balance-output]");

async function refreshBalance() {
    if (!accountSelect || !balanceOutput || !accountSelect.value) {
        return;
    }

    try {
        const response = await fetch(`/Transactions/Balance?accountId=${encodeURIComponent(accountSelect.value)}`);
        if (!response.ok) {
            return;
        }

        const data = await response.json();
        balanceOutput.textContent = `Available balance: ${data.currency} ${Number(data.balance).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
    } catch {
        balanceOutput.textContent = "Balance refresh is temporarily unavailable.";
    }
}

accountSelect?.addEventListener("change", refreshBalance);
form?.addEventListener("submit", () => setTimeout(refreshBalance, 750));
refreshBalance();
