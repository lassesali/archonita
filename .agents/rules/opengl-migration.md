# OpenGL 4.6 Core & OpenGL ES 3.2 Migration Rules

1. **Dual-Target Parity:** Every shader and GL call must compile cleanly on Desktop OpenGL 4.6 Core (`#version 460 core`) and Android OpenGL ES 3.2 (`#version 320 es`).
2. **No Raw DSA in Renderer Core:** OpenGL ES 3.2 does not support `GL_ARB_direct_state_access` (`glCreateBuffers`, `glNamedBufferData`, `glTextureSubImage2D`) in core. Always route buffer and texture state changes through our thin internal wrapper helpers.
3. **Mandatory VAO Binding:** Both GL 4.6 Core and GLES 3.2 require a bound Vertex Array Object (`glBindVertexArray`) for all draw calls. Never emit client-side vertex array pointers without a bound VAO.
4. **Automated Shader Verification:** Whenever creating or modifying a shader, validate both targets using `glslang` via shell tool before marking a task complete:
   - Desktop check: prepend `#version 460 core` and run `glslang --stdin -S <stage>`
   - Android check: prepend `#version 320 es\nprecision highp float;` and run `glslang --stdin -S <stage>`
   