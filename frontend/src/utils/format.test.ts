import { describe, expect, it } from 'vitest'
import { formatCop } from './format'

describe('formatCop', () => {
  it('formats COP values without decimal places', () => {
    const formatted = formatCop(1_250_000)

    expect(formatted).toContain('1.250.000')
  })
})
