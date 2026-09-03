import { describe, expect, it } from 'vitest'
import { formatCardExpiry, formatCardNumber, getCardBrand, onlyDigits } from './paymentCard'

describe('payment card helpers', () => {
  it('formatea y limita el número de tarjeta simulado', () => {
    expect(formatCardNumber('4242-4242 4242x424299')).toBe('4242 4242 4242 4242')
  })

  it('formatea el vencimiento y limita el código de seguridad', () => {
    expect(formatCardExpiry('1229')).toBe('12/29')
    expect(onlyDigits('1a23-45', 4)).toBe('1234')
  })

  it('muestra una marca visual sin validar la tarjeta contra servicios externos', () => {
    expect(getCardBrand('4242')).toBe('Visa')
    expect(getCardBrand('5555')).toBe('Mastercard')
    expect(getCardBrand('3782')).toBe('American Express')
    expect(getCardBrand('9999')).toBe('Nexus Card')
  })
})
