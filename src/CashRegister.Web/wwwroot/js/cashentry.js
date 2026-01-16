const DENOMINATIONS = [1000, 500, 200, 100, 50, 20, 10, 5, 2, 1];

document.addEventListener('DOMContentLoaded', function () {
    initializeCalculations();
    attachEventListeners();
});

function attachEventListeners() {
    document.querySelectorAll('.denomination-input').forEach(input => {
        input.addEventListener('input', function () {
            calculateAll();
        });
    });

    document.getElementById('saveBtn').addEventListener('click', saveDraft);
    document.getElementById('submitBtn').addEventListener('click', submitEntry);
}

function initializeCalculations() {
    calculateAll();
}

function calculateAll() {
    calculateRowTotals();
    const vaultClosingBalance = calculateVaultClosingBalance();
    const vaultFigure = calculateVaultFigure(vaultClosingBalance);
    calculateTillTotal(vaultClosingBalance, vaultFigure);
}

function calculateRowTotals() {
    const rows = document.querySelectorAll('#cashEntryTable tbody tr');

    rows.forEach(row => {
        const rowType = row.dataset.rowType;
        if (rowType === 'Vault Closing Balance' || rowType === 'Vault Figure' || rowType === 'Till Total') {
            return;
        }

        let total = 0;
        const inputs = row.querySelectorAll('.denomination-input');

        inputs.forEach(input => {
            const denomination = input.dataset.denomination;
            const value = parseFloat(input.value) || 0;

            if (denomination === 'coin') {
                total += value;
            } else {
                total += value;
            }
        });

        const totalInput = row.querySelector('.row-total');
        totalInput.value = formatCurrency(total);
    });
}

function calculateVaultClosingBalance() {
    const rows = document.querySelectorAll('#cashEntryTable tbody tr');
    const vaultClosingRow = Array.from(rows).find(r => r.dataset.rowType === 'Vault Closing Balance');

    const balances = {};
    DENOMINATIONS.forEach(d => balances[d] = 0);
    balances['coin'] = 0;

    rows.forEach(row => {
        const rowType = row.dataset.rowType;
        if (rowType === 'Vault Closing Balance' || rowType === 'Vault Figure' || rowType === 'Till Total') {
            return;
        }

        const isOutflow = row.dataset.isOutflow === 'true';
        const multiplier = isOutflow ? -1 : 1;

        const inputs = row.querySelectorAll('.denomination-input');
        inputs.forEach(input => {
            const denomination = input.dataset.denomination;
            const value = parseFloat(input.value) || 0;

            if (denomination === 'coin') {
                balances['coin'] += value * multiplier;
            } else {
                balances[denomination] += value * multiplier;
            }
        });
    });

    let total = 0;
    const closingInputs = vaultClosingRow.querySelectorAll('.denomination-input');
    closingInputs.forEach(input => {
        const denomination = input.dataset.denomination;
        const value = denomination === 'coin' ? balances['coin'] : balances[denomination];
        input.value = value;
        total += value;
    });

    const totalInput = vaultClosingRow.querySelector('.row-total');
    totalInput.value = formatCurrency(total);

    return balances;
}

function calculateVaultFigure(vaultClosingBalance) {
    const rows = document.querySelectorAll('#cashEntryTable tbody tr');
    const vaultFigureRow = Array.from(rows).find(r => r.dataset.rowType === 'Vault Figure');

    const figures = {};
    let totalFigure = 0;

    DENOMINATIONS.forEach(denomination => {
        const amount = vaultClosingBalance[denomination] || 0;
        const quantity = Math.floor(amount / denomination);
        const fullPacks = Math.floor(quantity / 100) * 100;
        const figureAmount = fullPacks * denomination;

        figures[denomination] = figureAmount;
        totalFigure += figureAmount;
    });

    figures['coin'] = 0;

    const figureInputs = vaultFigureRow.querySelectorAll('.denomination-input');
    figureInputs.forEach(input => {
        const denomination = input.dataset.denomination;
        const value = denomination === 'coin' ? 0 : (figures[denomination] || 0);
        input.value = value;
    });

    const totalInput = vaultFigureRow.querySelector('.row-total');
    totalInput.value = formatCurrency(totalFigure);

    return figures;
}

function calculateTillTotal(vaultClosingBalance, vaultFigure) {
    const rows = document.querySelectorAll('#cashEntryTable tbody tr');
    const tillTotalRow = Array.from(rows).find(r => r.dataset.rowType === 'Till Total');

    let total = 0;
    const tillInputs = tillTotalRow.querySelectorAll('.denomination-input');

    tillInputs.forEach(input => {
        const denomination = input.dataset.denomination;

        if (denomination === 'coin') {
            const coinValue = vaultClosingBalance['coin'] || 0;
            input.value = coinValue;
            total += coinValue;
        } else {
            const closingValue = vaultClosingBalance[denomination] || 0;
            const figureValue = vaultFigure[denomination] || 0;
            const tillValue = closingValue - figureValue;
            input.value = tillValue;
            total += tillValue;
        }
    });

    const totalInput = tillTotalRow.querySelector('.row-total');
    totalInput.value = formatCurrency(total);
}

function formatCurrency(value) {
    return value.toLocaleString('en-NG', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}

function parseCurrency(value) {
    if (!value) return 0;
    return parseFloat(value.replace(/,/g, '')) || 0;
}

function collectFormData() {
    const rows = [];
    const tableRows = document.querySelectorAll('#cashEntryTable tbody tr');

    tableRows.forEach((row, index) => {
        const inputs = row.querySelectorAll('.denomination-input');
        const rowData = {
            rowType: row.dataset.rowType,
            isOutflow: row.dataset.isOutflow === 'true',
            sequenceOrder: index + 1,
            amount1000: inputs[0] ? (parseFloat(inputs[0].value) || 0) : 0,
            amount500: inputs[1] ? (parseFloat(inputs[1].value) || 0) : 0,
            amount200: inputs[2] ? (parseFloat(inputs[2].value) || 0) : 0,
            amount100: inputs[3] ? (parseFloat(inputs[3].value) || 0) : 0,
            amount50: inputs[4] ? (parseFloat(inputs[4].value) || 0) : 0,
            amount20: inputs[5] ? (parseFloat(inputs[5].value) || 0) : 0,
            amount10: inputs[6] ? (parseFloat(inputs[6].value) || 0) : 0,
            amount5: inputs[7] ? (parseFloat(inputs[7].value) || 0) : 0,
            amount2: inputs[8] ? (parseFloat(inputs[8].value) || 0) : 0,
            amount1: inputs[9] ? (parseFloat(inputs[9].value) || 0) : 0,
            coinAmount: inputs[10] ? (parseFloat(inputs[10].value) || 0) : 0
        };
        rows.push(rowData);
    });

    const entryIdElement = document.getElementById('entryId');
    const entryDateElement = document.getElementById('entryDate');

    return {
        entryId: entryIdElement ? (entryIdElement.value || null) : null,
        entryDate: entryDateElement ? entryDateElement.value : '',
        rows: rows
    };
}

async function saveDraft() {
    const btn = document.getElementById('saveBtn');
    btn.disabled = true;
    btn.textContent = 'Saving...';

    try {
        const data = collectFormData();
        const headers = {
            'Content-Type': 'application/json'
        };

        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenElement) {
            headers['RequestVerificationToken'] = tokenElement.value;
        }

        const response = await fetch('/CashEntry?handler=Save', {
            method: 'POST',
            headers: headers,
            body: JSON.stringify(data)
        });

        const result = await response.json();

        if (result.success) {
            const entryIdElement = document.getElementById('entryId');
            if (entryIdElement) {
                entryIdElement.value = result.entryId;
            }
            showMessage('Draft saved successfully', 'success');
        } else {
            showMessage('Error: ' + result.error, 'danger');
        }
    } catch (error) {
        showMessage('Error saving draft: ' + error.message, 'danger');
    } finally {
        btn.disabled = false;
        btn.textContent = 'Save Draft';
    }
}

async function submitEntry() {
    if (!confirm('Are you sure you want to submit this entry for authorization? You will not be able to edit it after submission.')) {
        return;
    }

    const entryIdElement = document.getElementById('entryId');
    const entryId = entryIdElement ? entryIdElement.value : '';
    if (!entryId) {
        alert('Please save the entry first before submitting');
        return;
    }

    const btn = document.getElementById('submitBtn');
    btn.disabled = true;
    btn.textContent = 'Submitting...';

    try {
        const data = collectFormData();
        const headers = {
            'Content-Type': 'application/json'
        };

        const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenElement) {
            headers['RequestVerificationToken'] = tokenElement.value;
        }

        const response = await fetch('/CashEntry?handler=Submit', {
            method: 'POST',
            headers: headers,
            body: JSON.stringify(data)
        });

        const result = await response.json();

        if (result.success) {
            showMessage(result.message, 'success');
            setTimeout(() => {
                window.location.reload();
            }, 2000);
        } else {
            showMessage('Error: ' + result.error, 'danger');
        }
    } catch (error) {
        showMessage('Error submitting entry: ' + error.message, 'danger');
    } finally {
        btn.disabled = false;
        btn.textContent = 'Submit for Authorization';
    }
}

function showMessage(message, type) {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    const container = document.querySelector('.container-fluid');
    container.insertBefore(alertDiv, container.firstChild);

    setTimeout(() => {
        alertDiv.remove();
    }, 5000);
}
