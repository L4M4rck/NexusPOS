export type CardBrand = 'Visa' | 'Mastercard' | 'American Express' | 'Nexus Card'

export function onlyDigits(value: string, maximumLength: number) {
  return value.replace(/\D/g, '').slice(0, maximumLength)
}

export function formatCardNumber(value: string) {
  return onlyDigits(value, 16).replace(/(\d{4})(?=\d)/g, '$1 ')
}

export function formatCardExpiry(value: string) {
  const digits = onlyDigits(value, 4)
  return digits.length > 2 ? `${digits.slice(0, 2)}/${digits.slice(2)}` : digits
}

export function getCardBrand(value: string): CardBrand {
  const digits = onlyDigits(value, 16)
  if (digits.startsWith('4')) return 'Visa'
  if (/^5[1-5]/.test(digits)) return 'Mastercard'
  if (/^3[47]/.test(digits)) return 'American Express'
  return 'Nexus Card'
}
