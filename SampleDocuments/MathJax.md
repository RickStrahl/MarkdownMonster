# Math Expressions using MathJax
Markdown Monster has built in support for rendering Math expressions using Latex, MathML and AsciiMath syntax. 


In order to use Math expressions on your page you have to make sure that you enable the `Markdown.UseMathematics` setting to `true`.

### Syntax
MathJax supports several flavors of Math expressions and the MathJax settings provided with Markdown Monster support TeX, MML and HTML syntax.


### Block Expressions
The easiest and most common way to embed math expressions is via `$$` blocks, or `$` inlines. 

Using block expressions you can create Math like this:

$$
\frac{n!}{k!(n-k)!} = \binom{n}{k}
$$

which is created from:

```text
$$
\frac{n!}{k!(n-k)!} = \binom{n}{k}
$$
```

`$$` followed by a block of TeX markup is rendered as math expressions.


### Inline Expressions
You can use also inline expressions using a single `$` to delimit an expression:

---
Who hasn't heard of $E=mc^2$? 

This is another inline expression:  
$\vec{F} = \frac{d \vec{p}}{dt} = m \frac{d \vec{v}}{dt} = m \vec{a}$

---

**Created from:**

```text
Who hasn't heard of $E=mc^2$? 

This is another inline expression:  
$\vec{F} = \frac{d \vec{p}}{dt} = m \frac{d \vec{v}}{dt} = m \vec{a}$
```


### Mixed Inline and Block Expressions
The following is a mixture of inline and block operations:

---
When $a \ne 0$, there are two solutions to $ax^2 + bx + c = 0$ and they are

$$x = {-b \pm \sqrt{b^2-4ac} \over 2a}$$

---

**Created from:**   

```text
When $a \ne 0$, there are two solutions to $ax^2 + bx + c = 0$ and they are

$$x = {-b \pm \sqrt{b^2-4ac} \over 2a}$$
```

### HTML Math Markup
Once hooked up, you can add math expressions using `<div class="math">` syntax as follows:

<div class="math">
\begin{equation}
  \int_0^\infty \frac{x^3}{e^x-1}\,dx = \frac{\pi^4}{15}  
\end{equation}
</div>

**Created from:**

```html
<div class="math">
\begin{equation}
  \int_0^\infty \frac{x^3}{e^x-1}\,dx = \frac{\pi^4}{15}  
\end{equation}
</div>
```

**Note:** MathJax **requires** that a block operation is used to delimit the Math expression - in this example `\begin{equation}` and `\end{equation}`. Note also that you can simply write using the simpler `$$` or `$` syntax which is automatically converted by Markdown Monster into the above syntax.

### More Examples
A much longer expression:


<div class="math">
\begin{align}
\sqrt{37} & = \sqrt{\frac{73^2-1}{12^2}} \\
 & = \sqrt{\frac{73^2}{12^2}\cdot\frac{73^2-1}{73^2}} \\ 
 & = \sqrt{\frac{73^2}{12^2}}\sqrt{\frac{73^2-1}{73^2}} \\
 & = \frac{73}{12}\sqrt{1 - \frac{1}{73^2}} \\ 
 & \approx \frac{73}{12}\left(1 - \frac{1}{2\cdot73^2}\right)
\end{align}
</div>

**Created from:**

```html
<div class="math">
\begin{align}
\sqrt{37} & = \sqrt{\frac{73^2-1}{12^2}} \\
 & = \sqrt{\frac{73^2}{12^2}\cdot\frac{73^2-1}{73^2}} \\ 
 & = \sqrt{\frac{73^2}{12^2}}\sqrt{\frac{73^2-1}{73^2}} \\
 & = \frac{73}{12}\sqrt{1 - \frac{1}{73^2}} \\ 
 & \approx \frac{73}{12}\left(1 - \frac{1}{2\cdot73^2}\right)
\end{align}
</div>
```

**Output:**

$$
\sum_{\substack{
   0<i<m \\
   0<j<n
  }} 
 P(i,j)
$$

**Created from:**

```html
$$
\sum_{\substack{
   0<i<m \\
   0<j<n
  }} 
 P(i,j)
$$
```

**Output:**

$$
\frac{
    \begin{array}[b]{r}
      \left( x_1 x_2 \right)\\
      \times \left( x'_1 x'_2 \right)
    \end{array}
  }{
    \left( y_1y_2y_3y_4 \right)
  }
$$

**Created from:**

```text
$$
\frac{
    \begin{array}[b]{r}
      \left( x_1 x_2 \right)\\
      \times \left( x'_1 x'_2 \right)
    \end{array}
  }{
    \left( y_1y_2y_3y_4 \right)
  }
$$
```

### MathML
The following is a **MathML** block:

<math xmlns="http://www.w3.org/1998/Math/MathML" display="block">
  <msup>
    <mrow>
      <mo>(</mo>
      <munderover>
        <mo>&#x2211;<!-- ∑ --></mo>
        <mrow class="MJX-TeXAtom-ORD">
          <mi>k</mi>
          <mo>=</mo>
          <mn>1</mn>
        </mrow>
        <mi>n</mi>
      </munderover>
      <msub>
        <mi>a</mi>
        <mi>k</mi>
      </msub>
      <msub>
        <mi>b</mi>
        <mi>k</mi>
      </msub>
      <mo>)</mo>
    </mrow>
    <mrow class="MJX-TeXAtom-ORD">
      <mspace width="negativethinmathspace" />
      <mspace width="negativethinmathspace" />
      <mn>2</mn>
    </mrow>
  </msup>
  <mo>&#x2264;<!-- ≤ --></mo>
  <mrow>
    <mo>(</mo>
    <munderover>
      <mo>&#x2211;<!-- ∑ --></mo>
      <mrow class="MJX-TeXAtom-ORD">
        <mi>k</mi>
        <mo>=</mo>
        <mn>1</mn>
      </mrow>
      <mi>n</mi>
    </munderover>
    <msubsup>
      <mi>a</mi>
      <mi>k</mi>
      <mn>2</mn>
    </msubsup>
    <mo>)</mo>
  </mrow>
  <mrow>
    <mo>(</mo>
    <munderover>
      <mo>&#x2211;<!-- ∑ --></mo>
      <mrow class="MJX-TeXAtom-ORD">
        <mi>k</mi>
        <mo>=</mo>
        <mn>1</mn>
      </mrow>
      <mi>n</mi>
    </munderover>
    <msubsup>
      <mi>b</mi>
      <mi>k</mi>
      <mn>2</mn>
    </msubsup>
    <mo>)</mo>
  </mrow>
</math>

rendered from:

```xml
<math xmlns="http://www.w3.org/1998/Math/MathML" display="block">
  <msup>
    <mrow>
      <mo>(</mo>
      <munderover>
        <mo>&#x2211;<!-- ∑ --></mo>
        <mrow class="MJX-TeXAtom-ORD">
          <mi>k</mi>
          <mo>=</mo>
          <mn>1</mn>
        </mrow>
        <mi>n</mi>
      </munderover>
      <msub>
        <mi>a</mi>
        <mi>k</mi>
      </msub>
      <msub>
        <mi>b</mi>
        <mi>k</mi>
      </msub>
      <mo>)</mo>
    </mrow>
    <mrow class="MJX-TeXAtom-ORD">
      <mspace width="negativethinmathspace" />
      <mspace width="negativethinmathspace" />
      <mn>2</mn>
    </mrow>
  </msup>
  <mo>&#x2264;<!-- ≤ --></mo>
  <mrow>
    <mo>(</mo>
    <munderover>
      <mo>&#x2211;<!-- ∑ --></mo>
      <mrow class="MJX-TeXAtom-ORD">
        <mi>k</mi>
        <mo>=</mo>
        <mn>1</mn>
      </mrow>
      <mi>n</mi>
    </munderover>
    <msubsup>
      <mi>a</mi>
      <mi>k</mi>
      <mn>2</mn>
    </msubsup>
    <mo>)</mo>
  </mrow>
  <mrow>
    <mo>(</mo>
    <munderover>
      <mo>&#x2211;<!-- ∑ --></mo>
      <mrow class="MJX-TeXAtom-ORD">
        <mi>k</mi>
        <mo>=</mo>
        <mn>1</mn>
      </mrow>
      <mi>n</mi>
    </munderover>
    <msubsup>
      <mi>b</mi>
      <mi>k</mi>
      <mn>2</mn>
    </msubsup>
    <mo>)</mo>
  </mrow>
</math>
```